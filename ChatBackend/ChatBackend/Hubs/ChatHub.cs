using AutoMapper;
using ChatAppModels;
using ChatAppServer.Interfaces;
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
        private static readonly Dictionary<int, string> _connections = new();
        private readonly IChatRepository chatRepository;
        private readonly IFriendRepository friendRepository;
        private readonly IMapper mapper;

        public ChatHub(IChatRepository chatRepository, IFriendRepository friendRepository, IMapper mapper)
        {
            this.chatRepository = chatRepository;
            this.friendRepository = friendRepository;
            this.mapper = mapper;
        }

        public async Task SendMessage(MessageModel messageModel)
        {
            var message = mapper.Map<Message>(messageModel);
            message.DateCreate = DateTime.Now;

            var usersDialog = await chatRepository.GetUsersDialog(message.DialogId);
            for (int i = 0; i < usersDialog.Length; i++)
            {
                if (_connections.TryGetValue(usersDialog[i], out string connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }

            await chatRepository.CreateMessage(message);
            await chatRepository.SaveChangesAsync();
        }

        public async Task CheckDialog(int dialogId)
        {
            await chatRepository.CheckDialog(dialogId);
        }

        public async Task AddFriend(int toUserId)
        {
            var userId = Convert.ToInt32(Context.User.Identity.Name);
            if (_connections.TryGetValue(toUserId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveEventFriend", toUserId);
            }

            await friendRepository.Create(new Friend { UserId = userId, ToUserId = toUserId });
        }

        public override Task OnConnectedAsync()
        {
            var name = int.Parse(Context.User.Identity.Name);

            _connections.TryAdd(name, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = int.Parse(Context.User.Identity.Name);

            _connections.Remove(name);

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