using HelpdeskSaaS.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HelpdeskSaaS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(
            int userId,
            string message)
        {
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message);
        }

        public async Task SendToUsersAsync(
            IEnumerable<int> userIds,
            string message)
        {
            foreach (var userId in userIds.Distinct())
            {
                await SendToUserAsync(userId, message);
            }
        }
    }
}