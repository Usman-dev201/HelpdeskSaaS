using HelpdeskSaaS.Data;
using HelpdeskSaaS.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpdeskSaaS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketStatusHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketStatusHistoryController(
            ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetTicketHistory(
            int ticketId)
        {
           

            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;

            if (!int.TryParse(
                    userIdClaim,
                    out int userId))
            {
                return Unauthorized();
            }



            var currentUser =
                await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        u => u.UserId == userId
                    );

            if (currentUser == null)
            {
                return Unauthorized();
            }


         

            var ticket =
                await _context.Tickets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        t => t.TicketId == ticketId
                    );

            if (ticket == null)
            {
                return NotFound(
                    "Ticket not found."
                );
            }



            if (ticket.TenantId != currentUser.TenantId)
            {
                return Forbid();
            }


        

            if (
                currentUser.Role ==
                    UserRole.Customer
                &&
                ticket.CreatedByUserId != userId
            )
            {
                return Forbid();
            }



            if (
                currentUser.Role ==
                    UserRole.Agent
                &&
                ticket.AssignedAgentId != userId
            )
            {
                return Forbid();
            }


       
            var history =
                await _context.TicketStatusHistories
                    .AsNoTracking()
                    .Where(h =>
                        h.TicketId == ticketId
                    )
                    .OrderByDescending(
                        h => h.ChangedAt
                    )
                    .Select(h => new
                    {
                        historyId =
                            h.HistoryId,

                        oldStatus =
                            h.OldStatus.ToString(),

                        newStatus =
                            h.NewStatus.ToString(),

                        changedAt =
                            h.ChangedAt,

                        changedByUserId =
                            h.ChangedByUserId,

                        changedByUserName =
                            h.ChangedByUser.UserName
                    })
                    .ToListAsync();



            return Ok(history);
        }
    }
}