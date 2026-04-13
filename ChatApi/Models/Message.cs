using System.ComponentModel.DataAnnotations;

namespace ChatApi.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
