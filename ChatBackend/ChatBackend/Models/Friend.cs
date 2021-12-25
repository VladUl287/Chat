namespace ChatAppModels
{
    public class Friend
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ToUserId { get; set; }
        public User ToUser { get; set; }
        public bool IsConfirmed { get; set; }
    }
}