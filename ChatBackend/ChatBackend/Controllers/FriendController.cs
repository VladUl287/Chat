using ChatAppServer.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository friend;

        public FriendController(IFriendRepository friend)
        {
            this.friend = friend;
        }

        [HttpGet("{id}")]
        public async Task<IEnumerable<UserModel>> GetAll(int id)
        {
            return await friend.GetAll(id);
        }

        [HttpGet("outgoing/{id}")]
        public async Task<IEnumerable<UserModel>> GetOutgoing(int id)
        {
            return await friend.GetOutgoing(id);
        }

        [HttpGet("incoming/{id}")]
        public async Task<IEnumerable<UserModel>> GetIncoming(int id)
        {
            return await friend.GetIncoming(id);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch(AcceptModel acceptModel)
        {
            await friend.Accept(acceptModel.Id, acceptModel.FromId);

            return NoContent();
        }
    }
}