using System.ComponentModel.DataAnnotations;

namespace ChatBackend.ViewModels
{
    public class AcceptModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int FromId { get; set; }
    }
}