namespace ChatAppModels
{
    public class UserDialog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int DialogId { get; set; }
        public Dialog Dialog { get; set; }
    }
}