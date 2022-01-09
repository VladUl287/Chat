using ChatAppModels;
using ChatBackend.Database.Interfaces;
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
        private readonly IDialogRepository dialogRepository;
        private readonly IMessageRepository messageRepository;

        public ChatController(IDialogRepository dialogRepo, IMessageRepository messageRepo)
        {
            dialogRepository = dialogRepo;
            messageRepository = messageRepo;
        }

        [HttpGet("dialog/count/{id}")]
        public async Task<int> GetCount([FromRoute] int id)
        {
            return await dialogRepository.CountDialogs(id);
        }

        [HttpGet("dialog/{userId}/{toUserId}")]
        public async Task<int> GetDialogIdentifier([FromRoute] int userId, [FromRoute] int toUserId)
        {
            return await dialogRepository.GetDialogIdentifier(userId, toUserId);
        }

        [HttpGet("dialog/{id}")]
        public async Task<DialogModel> GetDialog([FromRoute] int id)
        {
            var userId = int.Parse(User.Identity.Name);

            return await dialogRepository.GetDialogView(userId, id);
        }

        [HttpGet("dialog/users/{dialogId}")]
        public async Task<IEnumerable<UserModel>> GetUsersModel([FromRoute] int dialogId)
        {
            return await dialogRepository.GetUsersDialog(dialogId);
        }

        [HttpGet("messages/{dialogId}")]
        public async Task<IEnumerable<Message>> GetMessages([FromRoute] int dialogId)
        {
            await dialogRepository.CheckDialog(dialogId);

            return await messageRepository.GetMessages(dialogId);
        }

        [HttpGet("dialogs/{userId}")]
        public async Task<IEnumerable<DialogModel>> GetDialogs([FromRoute] int userId)
        {
            return await dialogRepository.GetDialogs(userId);
        }

        [HttpPost("create/dialog")]
        public async Task<IActionResult> CreateDialog([FromForm] CreateDialogModel createDialog)
        {
            var dialog = new Dialog
            {
                Name = createDialog.Name,
                UserId = createDialog.UserId,
                IsMultiple = true,
            };

            if (createDialog.FacialImage is not null && createDialog.FacialImage.Length > 0)
            {
                dialog.Image = await ImageConverter.GetBase64(createDialog.FacialImage, 80, 60);
            }

            await dialogRepository.CreateDialog(dialog, createDialog.UsersId);
            await dialogRepository.SaveChangesAsync();

            return CreatedAtRoute(nameof(GetDialog), new { id = dialog.Id }, dialog);
        }

        [HttpDelete("dialog/{id}")]
        public async Task<IActionResult> DeleteDialog([FromRoute] int id)
        {
            var userId = int.Parse(User.Identity.Name);

            await dialogRepository.DeleteDialog(userId, id);

            return NoContent();
        }

        [HttpDelete("messages")]
        public async Task<IActionResult> DeleteMessages(int[] arrId)
        {
            await messageRepository.DeleteMessages(arrId);
            await messageRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}