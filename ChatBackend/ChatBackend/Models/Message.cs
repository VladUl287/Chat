using System;

namespace ChatAppModels
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DialogId { get; set; }
        public string Content { get; set; }
        public DateTime DateCreate { get; set; }
        public bool IsRead { get; set; }
    }
}