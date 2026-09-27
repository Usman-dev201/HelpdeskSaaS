using HelpdeskSaaS.Enums;

namespace HelpdeskSaaS.Models
{
    public class TicketStatusHistory
    {
        public int HistoryId { get; set; }

        public TicketStatus OldStatus { get; set; }

        public TicketStatus NewStatus { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;


       
        public int TicketId { get; set; }

        public Ticket Ticket { get; set; } = null!;


     
        public int ChangedByUserId { get; set; }

        public User ChangedByUser { get; set; } = null!;
    }

}
