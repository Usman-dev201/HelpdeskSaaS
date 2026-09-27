using HelpdeskSaaS.Enums;

namespace HelpdeskSaaS.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public int TenantId { get; set; }

        public Tenant? Tenant { get; set; }

        public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        public ICollection<TicketStatusHistory> StatusChanges { get; set; } = new List<TicketStatusHistory>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
