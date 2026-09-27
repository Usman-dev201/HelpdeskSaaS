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


        // =====================================================
        // GET STATUS HISTORY BY TICKET
        // =====================================================

        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetTicketHistory(
            int ticketId)
        {
            // =================================================
            // GET CURRENT USER ID FROM JWT
            // =================================================

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


            // =================================================
            // GET CURRENT USER
            // =================================================

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


            // =================================================
            // GET TICKET
            // =================================================

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


            // =================================================
            // TENANT SECURITY
            // =================================================

            if (ticket.TenantId != currentUser.TenantId)
            {
                return Forbid();
            }


            // =================================================
            // CUSTOMER SECURITY
            // =================================================

            // Customer can only see history
            // of tickets created by themselves.

            if (
                currentUser.Role ==
                    UserRole.Customer
                &&
                ticket.CreatedByUserId != userId
            )
            {
                return Forbid();
            }


            // =================================================
            // AGENT SECURITY
            // =================================================

            // Agent can only see history
            // of tickets assigned to themselves.

            if (
                currentUser.Role ==
                    UserRole.Agent
                &&
                ticket.AssignedAgentId != userId
            )
            {
                return Forbid();
            }


            // =================================================
            // GET STATUS HISTORY
            // =================================================

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


            // =================================================
            // RETURN RESPONSE
            // =================================================

            return Ok(history);
        }
    }
}