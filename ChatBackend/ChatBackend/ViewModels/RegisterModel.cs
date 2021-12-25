using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ChatBackend.ViewModels
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; }

        [Required]
        public string Password { get; set; }
    }
}