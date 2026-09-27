using HelpdeskSaaS.Data;
using HelpdeskSaaS.Hubs;
using HelpdeskSaaS.Models;
using Microsoft.AspNetCore.SignalR;

namespace HelpdeskSaaS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task SendToUserAsync(
    int userId,
    string message)
        {
           

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();



            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync(
                    "ReceiveNotification",
                    new
                    {
                        notificationId =
                            notification.NotificationId,

                        message =
                            notification.Message,

                        createdAt =
                            notification.CreatedAt,

                        isRead =
                            notification.IsRead
                    }
                );
        }


        public async Task SendToUsersAsync(
            IEnumerable<int> userIds,
            string message)
        {
            foreach (var userId in userIds.Distinct())
            {
                await SendToUserAsync(
                    userId,
                    message
                );
            }
        }
    }
}