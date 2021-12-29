﻿using AutoMapper;
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

        [HttpGet("dialog/check/{dialogId}")]
        public async Task CheckDialog(int dialogId)
        {
            await chat.CheckDialog(dialogId);
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
        public async Task<IActionResult> CreateDialog(CreateDialogModel createDialog)
        {
            var dialog = mapper.Map<Dialog>(createDialog);
            dialog.IsMultiple = true;

            var created = await chat.CreateDialog(dialog);

            await chat.AddUsersDialog(createDialog.UsersId, created.Id);
            await chat.SaveChangesAsync();

            return Ok();
        }
    }
}