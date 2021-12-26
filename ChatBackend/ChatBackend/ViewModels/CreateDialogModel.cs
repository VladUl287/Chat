namespace ChatBackend.ViewModels
{
    public class CreateDialogModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public int UserId { get; set; }

        public int[] UsersId { get; set; }
    }
}
