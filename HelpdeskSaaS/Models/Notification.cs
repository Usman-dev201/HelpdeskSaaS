using System.Text.Json.Serialization;

namespace HelpdeskSaaS.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }
      
        public User User { get; set; } = null!;
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
