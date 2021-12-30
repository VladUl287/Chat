using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet("all/{id}")]
        public async Task<IEnumerable<UserModel>> GetAll(int id)
        {
            return await userRepository.GetAll(id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserPageModel>> GetUser(int id)
        {
            var authId = int.Parse(User.Identity.Name);
            var user = await userRepository.Get(authId, id);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        [Route("search/{login}")]
        public async Task<IEnumerable<UserModel>> SearchUsers(string login)
        {
            var id = int.Parse(User.Identity.Name);

            return await userRepository.Search(id, login);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await userRepository.Get(id);

            if (user is not null)
            {
                await userRepository.Delete(user);
            }

            return NoContent();
        }
    }
}