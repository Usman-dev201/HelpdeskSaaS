using HelpdeskSaaS.Enums;

namespace HelpdeskSaaS.DTOs
{
    public class CreateTicketDto
    {
        public string TicketTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public string Category { get; set; } = string.Empty;
    }
}
