using HelpdeskSaaS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpdeskSaaS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var notifications =
     await _context.Notifications
         .Where(n =>
             n.UserId == userId &&
             !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Select(n => new
         {
             n.NotificationId,
             n.Message,
             n.IsRead,
             n.CreatedAt
         })
         .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id &&
                    n.UserId == userId);

            if (notification == null)
                return NotFound("Notification not found.");

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok("Notification marked as read.");
        }

        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var notifications = await _context.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok("All notifications marked as read.");
        }
    }
}
