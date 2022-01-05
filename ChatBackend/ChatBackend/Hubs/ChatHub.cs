using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.Database.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<int> _connections = new();
        private readonly IMessageRepository chatRepository;
        private readonly IDialogRepository dialogRepository;
        private readonly IFriendRepository friendRepository;

        public ChatHub(IMessageRepository chatRepository, IDialogRepository dialogRepository, IFriendRepository friendRepository)
        {
            this.chatRepository = chatRepository;
            this.dialogRepository = dialogRepository;
            this.friendRepository = friendRepository;
        }

        public async Task SendMessage(MessageModel messageModel)
        {
            var message = new Message
            {
                Content = messageModel.Content,
                UserId = messageModel.UserId,
                DialogId = messageModel.DialogId,
                DateCreate = DateTime.Now
            };

            var usersDialog = await dialogRepository.GetUsersIdentifier(message.DialogId);
            for (int i = 0; i < usersDialog.Length; i++)
            {
                foreach (var connectionId in _connections.GetConnections(usersDialog[i]))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }

            await chatRepository.CreateMessage(message);
            await chatRepository.SaveChangesAsync();
        }

        public async Task CheckDialog(int dialogId)
        {
            await dialogRepository.CheckDialog(dialogId);
        }

        public async Task AddFriend(int toUserId)
        {
            var userId = int.Parse(Context.User.Identity.Name);

            var connections = _connections.GetConnections(userId).ToList();
            if (connections.Count > 0)
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveEventFriend", toUserId);
                }
            }

            await friendRepository.Create(new Friend { UserId = userId, ToUserId = toUserId });
        }

        public override Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User.Identity.Name);

            _connections.Add(userId, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = int.Parse(Context.User.Identity.Name);

            _connections.Remove(userId, Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections = new();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out HashSet<string> connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            if (_connections.TryGetValue(key, out HashSet<string> connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out HashSet<string> connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}