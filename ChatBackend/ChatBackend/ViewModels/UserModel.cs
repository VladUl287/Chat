namespace ChatBackend.ViewModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public bool IsFriend { get; set; } = false;
        public bool IsSender { get; set; } = false;
        public bool IsReceiver { get; set; } = false;
    }
}