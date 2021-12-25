using System.Collections.Generic;

namespace ChatAppModels
{
    public class Dialog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public bool IsMultiple { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}