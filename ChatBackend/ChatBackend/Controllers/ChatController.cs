using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository chat;

        public ChatController(IChatRepository chat)
        {
            this.chat = chat;
        }

        [HttpGet("{dialogId}")]
        public async Task<IEnumerable<Message>> GetMessages([FromRoute] int dialogId)
        {
            return await chat.GetMessages(dialogId);
        }

        [HttpGet("{userId}/{toUserId}")]
        public async Task<IEnumerable<Message>> GetMessages(int userId, int toUserId)
        {
            var dialog = await chat.GetDialog(userId, toUserId);

            return await chat.GetMessages(dialog.Id);
        }

        [HttpGet("{id}")]
        public async Task<IEnumerable<DialogModel>> GetDialogs([FromRoute] int userId)
        {
            return await chat.GetDialogs(userId);
        }
    }
}