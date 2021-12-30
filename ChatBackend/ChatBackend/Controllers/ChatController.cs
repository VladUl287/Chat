using AutoMapper;
using ChatAppModels;
using ChatAppServer.Interfaces;
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
        private readonly IChatRepository chat;
        private readonly IMapper mapper;

        public ChatController(IChatRepository chat, IMapper mapper)
        {
            this.chat = chat;
            this.mapper = mapper;
        }

        [HttpGet("count/{userId}")]
        public async Task<int> GetCount([FromRoute] int userId)
        {
            return await chat.CountDialogs(userId);
        }

        [HttpGet("dialog/{userId}/{toUserId}")]
        public async Task<Dialog> GetDialog(int userId, int toUserId)
        {
            var dialog = await chat.GetDialog(userId, toUserId);

            return dialog;
        }

        [HttpGet("messages/{dialogId}")]
        public async Task<IEnumerable<Message>> GetMessages([FromRoute] int dialogId)
        {
            await chat.CheckDialog(dialogId);

            return await chat.GetMessages(dialogId);
        }

        [HttpGet("dialogs/{userId}")]
        public async Task<IEnumerable<DialogModel>> GetDialogs([FromRoute] int userId)
        {
            return await chat.GetDialogs(userId);
        }

        [HttpPost("create/dialog")]
        public async Task<IActionResult> CreateDialog([FromForm] CreateDialogModel createDialog)
        {
            var dialog = mapper.Map<Dialog>(createDialog);
            dialog.IsMultiple = true;

            if (createDialog.FacialImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await createDialog.FacialImage.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var img = await Image.LoadAsync(memoryStream);
                using var memory = new MemoryStream();
                img.Mutate(x => x.Resize(80, 60, KnownResamplers.Lanczos3));
                await img.SaveAsync(memory, new JpegEncoder());
                var fileBytes = memory.ToArray();
                dialog.Image = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
            }

            var created = await chat.CreateDialog(dialog);

            await chat.AddUsersDialog(createDialog.UsersId, created.Id);
            await chat.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("dialog/{id}")]
        public async Task<IActionResult> DeleteDialog([FromRoute] int id)
        {
            await chat.DeleteUserDialog(id);
            await chat.DeleteDialog(id);
            await chat.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("messages")]
        public async Task<IActionResult> DeleteMessages(int[] arrId)
        {
            await chat.DeleteMessages(arrId);

            await chat.SaveChangesAsync();

            return Ok();
        }
    }
}