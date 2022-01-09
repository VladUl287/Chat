using ChatAppModels;
using ChatBackend.Database.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
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
            var dialog = await dialogRepository.GetDialogIdentifier(userId, toUserId);

            return dialog;
        }

        [HttpGet("dialog/{id}")]
        public async Task<DialogModel> GetDialog([FromRoute] int id)
        {
            var userId = int.Parse(User.Identity.Name);

            var dialog = await dialogRepository.GetDialogView(userId, id);

            return dialog;
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
                using var memoryStream = new MemoryStream();
                await createDialog.FacialImage.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var img = await Image.LoadAsync(memoryStream);
                using var memory = new MemoryStream();
                img.Mutate(x => x.Resize(80, 60, KnownResamplers.Lanczos3));
                await img.SaveAsync(memory, new JpegEncoder());
                var fileBytes = Convert.ToBase64String(memory.ToArray());
                dialog.Image = $"data:image/jpeg;base64,{fileBytes}";
            }

            await dialogRepository.CreateDialog(dialog, createDialog.UsersId);
            await dialogRepository.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("dialog/{id}")]
        public async Task<IActionResult> DeleteDialog([FromRoute] int id)
        {
            var userId = int.Parse(User.Identity.Name);

            await dialogRepository.DeleteDialog(userId, id);

            return Ok();
        }

        [HttpDelete("messages")]
        public async Task<IActionResult> DeleteMessages(int[] arrId)
        {
            await messageRepository.DeleteMessages(arrId);
            await messageRepository.SaveChangesAsync();

            return Ok();
        }
    }
}