namespace HelpdeskSaaS.DTOs
{
    public class RegisterTenantDto
    {
        public string TenantName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
