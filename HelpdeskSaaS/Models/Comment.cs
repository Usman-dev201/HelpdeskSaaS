namespace HelpdeskSaaS.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Ticket relationship
        public int TicketId { get; set; }

        public Ticket Ticket { get; set; } = null!;
        // User relationship
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
