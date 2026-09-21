using HelpdeskSaaS.Enums;

namespace HelpdeskSaaS.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        public string TicketTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public string Category { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public int? AssignedAgentId { get; set; }

        public User? AssignedAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TicketStatusHistory> StatusHistories { get; set; } = new List<TicketStatusHistory>();

    }
}
