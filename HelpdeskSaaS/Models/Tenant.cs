namespace HelpdeskSaaS.Models
{
    public class Tenant
    {
        public int TenantId { get; set; }

        public string TenantName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
