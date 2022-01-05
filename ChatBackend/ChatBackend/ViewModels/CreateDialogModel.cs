using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ChatBackend.ViewModels
{
    public class CreateDialogModel
    {
        [Required]
        public string Name { get; set; }

        public IFormFile FacialImage { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int[] UsersId { get; set; }
    }
}
