using System.ComponentModel.DataAnnotations;

namespace ChatBackend.ViewModels
{
    public class AuthModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}