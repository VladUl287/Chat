using System;

namespace ChatBackend.ViewModels
{
    public class DialogModel
    {
        public int Id { get; set; }
        public bool IsMultiple { get; set; }
        public int LastUserId { get; set; }
        public string LastMessage { get; set; }
        public DateTime DateTime { get; set; }
        public string Login { get; set; }
        public string Image { get; set; }
        public bool IsConfirm { get; set; }
    }
}