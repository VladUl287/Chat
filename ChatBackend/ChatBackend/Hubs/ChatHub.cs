using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.Database.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChatBackend.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private static readonly ConnectionMapping<int> _connections = new();
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

            await chatRepository.CreateMessage(message);
            await chatRepository.SaveChangesAsync();

            var usersDialog = await dialogRepository.GetUsersIdentifier(message.DialogId);

            for (int i = 0; i < usersDialog.Length; i++)
            {
                var connections = _connections.GetConnections(usersDialog[i]);
                await Clients.Clients(connections).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task CheckDialog(int dialogId)
        {
            await dialogRepository.CheckDialog(dialogId);
        }

        public async Task AddFriend(int toUserId)
        {
            var userId = int.Parse(Context.User.Identity.Name);

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
}