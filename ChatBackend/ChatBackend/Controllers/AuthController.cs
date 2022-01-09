using AutoMapper;
using ChatAppModels;
using ChatBackend.Database;
using ChatBackend.ViewModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly DatabaseContext dbContext;
        private readonly IMapper mapper;

        public AuthController(DatabaseContext dbContext, IConfiguration configuration, IMapper mapper)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] AuthModel auth)
        {
            string salt = configuration.GetValue<string>("Secrets:PasswordSalt");
            string passwordHash = HashPassword(auth.Password, salt);

            var user = await dbContext.Users
                .AsNoTracking()
                .Select(x => new 
                { 
                    x.Id,
                    x.Email, 
                    x.Password, 
                    RoleName = x.Role.Name, 
                })
                .FirstOrDefaultAsync(e => e.Email == auth.Email);

            if (user is null || user.Password != passwordHash)
            {
                return BadRequest(error: "Неверный email или пароль.");
            }

            var claims = new Claim[]
                {
                    new Claim("email", user.Email),
                    new Claim("role", user.RoleName),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
                };

            var secret = configuration.GetValue<string>("Secrets:JwtSecret");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel register)
        {
            var exists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(e => e.Email == register.Email);

            if (exists)
            {
                return BadRequest("Пользователь с таким email уже зарегистрирован.");
            }

            var newUser = mapper.Map<User>(register);

            if (register.ImageFile.Length > 0)
            {
                newUser.Image = await GetBase64(register.ImageFile, 160, 120);
            }

            string salt = configuration.GetValue<string>("Secrets:PasswordSalt");
            newUser.Password = HashPassword(register.Password, salt);

            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        public static async Task<string> GetBase64(IFormFile image, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var img = await Image.LoadAsync(memoryStream);
            using var memory = new MemoryStream();
            img.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));
            await img.SaveAsync(memory, new JpegEncoder());
            var fileBytes = memory.ToArray();
            return $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] saltArr = Encoding.ASCII.GetBytes(salt);

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltArr,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}