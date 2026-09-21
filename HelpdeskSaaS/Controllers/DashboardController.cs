using System.Security.Claims;
using HelpdeskSaaS.Data;
using HelpdeskSaaS.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSaaS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
           
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

            var currentAdmin = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

         
            var tickets = _context.Tickets
                .AsNoTracking()
                .Where(t => t.TenantId == currentAdmin.TenantId);

     
            var stats = await tickets
                .GroupBy(t => 1)
                .Select(g => new
                {
                    totalTickets = g.Count(),

                    openTickets = g.Count(t =>
                        t.Status == TicketStatus.Open),

                    inProgressTickets = g.Count(t =>
                        t.Status == TicketStatus.InProgress),

                    resolvedTickets = g.Count(t =>
                        t.Status == TicketStatus.Resolved),

                    closedTickets = g.Count(t =>
                        t.Status == TicketStatus.Closed)
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                return Ok(new
                {
                    totalTickets = 0,
                    openTickets = 0,
                    inProgressTickets = 0,
                    resolvedTickets = 0,
                    closedTickets = 0
                });
            }

            return Ok(stats);
        }

        // GET: api/Dashboard/priority-stats
        [HttpGet("priority-stats")]
        public async Task<IActionResult> GetPriorityStats()
        {
            
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

        
            var currentAdmin = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

           
            var tickets = _context.Tickets
                .AsNoTracking()
                .Where(t => t.TenantId == currentAdmin.TenantId);

     
            var stats = await tickets
                .GroupBy(t => 1)
                .Select(g => new
                {
                    low = g.Count(t =>
                        t.Priority == TicketPriority.Low),

                    medium = g.Count(t =>
                        t.Priority == TicketPriority.Medium),

                    high = g.Count(t =>
                        t.Priority == TicketPriority.High),

                    urgent = g.Count(t =>
                        t.Priority == TicketPriority.Urgent)
                })
                .FirstOrDefaultAsync();

        
            if (stats == null)
            {
                return Ok(new
                {
                    low = 0,
                    medium = 0,
                    high = 0,
                    urgent = 0
                });
            }

            return Ok(stats);
        }

        // GET: api/Dashboard/category-stats
        [HttpGet("category-stats")]
        public async Task<IActionResult> GetCategoryStats()
        {
   
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

            var currentAdmin = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

           
            var categoryStats = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.TenantId == currentAdmin.TenantId)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    category = g.Key,
                    ticketCount = g.Count()
                })
                .OrderByDescending(x => x.ticketCount)
                .ToListAsync();

            return Ok(categoryStats);
        }

        // GET: api/Dashboard/agent-performance
        [HttpGet("agent-performance")]
        public async Task<IActionResult> GetAgentPerformance()
        {
            
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

            var currentAdmin = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

            var performance = await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.TenantId == currentAdmin.TenantId &&
                    u.Role == UserRole.Agent)
                .Select(u => new
                {
                    agentId = u.UserId,
                    agentName = u.UserName,
                    email = u.Email,

                    assignedTickets = _context.Tickets.Count(t =>
                        t.TenantId == currentAdmin.TenantId &&
                        t.AssignedAgentId == u.UserId),

                    resolvedTickets = _context.Tickets.Count(t =>
                        t.TenantId == currentAdmin.TenantId &&
                        t.AssignedAgentId == u.UserId &&
                        t.Status == TicketStatus.Resolved),

                    closedTickets = _context.Tickets.Count(t =>
                        t.TenantId == currentAdmin.TenantId &&
                        t.AssignedAgentId == u.UserId &&
                        t.Status == TicketStatus.Closed)
                })
                .ToListAsync();

            return Ok(performance);
        }
    }
}