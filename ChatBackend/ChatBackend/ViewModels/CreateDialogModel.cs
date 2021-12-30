using Microsoft.AspNetCore.Http;

namespace ChatBackend.ViewModels
{
    public class CreateDialogModel
    {
        public string Name { get; set; }

        public IFormFile FacialImage { get; set; }

        public int UserId { get; set; }

        public int[] UsersId { get; set; }
    }
}
