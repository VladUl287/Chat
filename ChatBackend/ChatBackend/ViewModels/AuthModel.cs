using System.ComponentModel.DataAnnotations;

namespace ChatBackend.ViewModels
{
    public class AuthModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}