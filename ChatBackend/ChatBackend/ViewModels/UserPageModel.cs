namespace ChatBackend.ViewModels
{
    public class UserPageModel : UserModel
    {
        public bool IsSender { get; set; } = false;
        public bool IsReceiver { get; set; } = false;
    }
}
