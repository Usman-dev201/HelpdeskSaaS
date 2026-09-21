using HelpdeskSaaS.Enums;

namespace HelpdeskSaaS.DTOs
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; }
    }
}
