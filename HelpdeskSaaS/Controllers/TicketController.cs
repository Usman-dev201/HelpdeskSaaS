using HelpdeskSaaS.Data;
using HelpdeskSaaS.DTOs;
using HelpdeskSaaS.Enums;
using HelpdeskSaaS.Models;
using HelpdeskSaaS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpdeskSaaS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public TicketsController(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTicketsForAdmin()
        {
          
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

          
            var currentAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

        
            var tickets = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.TenantId == currentAdmin.TenantId)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedAgent)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketTitle,
                    t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    t.Category,

                    Customer = new
                    {
                        t.CreatedByUserId,
                        UserName = t.CreatedByUser!.UserName,
                        Email = t.CreatedByUser.Email
                    },

                    AssignedAgent = t.AssignedAgent == null
                        ? null
                        : new
                        {
                            AgentId = t.AssignedAgent.UserId,
                            UserName = t.AssignedAgent.UserName,
                            Email = t.AssignedAgent.Email
                        },

                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");

            var query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedAgent)
                .Where(t => t.TenantId == currentUser.TenantId);


     

            if (currentUser.Role == UserRole.Customer)
            {
                query = query.Where(t =>
                    t.CreatedByUserId == currentUser.UserId);
            }


           

            else if (currentUser.Role == UserRole.Agent)
            {
                query = query.Where(t =>
                    t.AssignedAgentId == currentUser.UserId);
            }


           


            var tickets = await query
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketTitle,
                    t.Description,

                    Status = t.Status.ToString(),

                    Priority = t.Priority.ToString(),

                    t.Category,

                    CreatedByUserId = t.CreatedByUserId,

                    CreatedByUserName =
                        t.CreatedByUser != null
                            ? t.CreatedByUser.UserName
                            : null,

                    CreatedByUserEmail =
                        t.CreatedByUser != null
                            ? t.CreatedByUser.Email
                            : null,

                    AssignedAgentId = t.AssignedAgentId,

                    AssignedAgentName =
                        t.AssignedAgent != null
                            ? t.AssignedAgent.UserName
                            : null,

                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();


            return Ok(tickets);
        }



        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

           
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");

      
            var ticket = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedAgent)
                .Where(t =>
                    t.TicketId == id &&
                    t.CreatedByUser!.TenantId == currentUser.TenantId)
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketTitle,
                    t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    t.Category,

                    CreatedByUserId = t.CreatedByUserId,
                    CreatedByUserName = t.CreatedByUser != null
                        ? t.CreatedByUser.UserName
                        : null,

                    AssignedAgentId = t.AssignedAgentId,
                    AssignedAgentName = t.AssignedAgent != null
                        ? t.AssignedAgent.UserName
                        : null,

                    t.CreatedAt,
                    t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound("Ticket not found.");


            if (currentUser.Role == UserRole.Customer &&
                ticket.CreatedByUserId != currentUser.UserId)
            {
                return NotFound("Ticket not found.");
            }

            return Ok(ticket);
        }
        // POST: api/tickets
        [HttpPost]
        public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized("User not found.");

            if (string.IsNullOrWhiteSpace(dto.TicketTitle) ||
                string.IsNullOrWhiteSpace(dto.Description) ||
                string.IsNullOrWhiteSpace(dto.Category))
            {
                return BadRequest(
                    "TicketTitle, Description and Category are required.");
            }

            var ticket = new Ticket
            {
                TicketTitle = dto.TicketTitle,
                Description = dto.Description,
                Priority = dto.Priority,
                Category = dto.Category,

                CreatedByUserId = userId,

                TenantId = user.TenantId,

                Status = Enums.TicketStatus.Open,

                AssignedAgentId = null,

                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

        
            var notificationUserIds = await _context.Users
                .Where(u =>
                    u.TenantId == user.TenantId &&
                    u.Role == Enums.UserRole.Admin &&
                    u.UserId != userId)
                .Select(u => u.UserId)
                .ToListAsync();

            if (notificationUserIds.Any())
            {
                await _notificationService.SendToUsersAsync(
                    notificationUserIds,
                    $"New ticket created: {ticket.TicketTitle}"
                );
            }

            return Ok("Ticket created successfully.");
        }
        // PUT: api/tickets/{id}/assign
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTicket(
            int id,
            AssignTicketDto dto)
        {
          
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

          
            var currentAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

       
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == id &&
                    t.TenantId == currentAdmin.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");

          
            var agent = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == dto.AgentId &&
                    u.TenantId == currentAdmin.TenantId &&
                    u.Role == UserRole.Agent);

            if (agent == null)
                return BadRequest("Agent not found in your tenant.");

            
            ticket.AssignedAgentId = agent.UserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

         
            await _notificationService.SendToUserAsync(
                agent.UserId,
                $"Ticket \"{ticket.TicketTitle}\" has been assigned to you.");

            return Ok(new
            {
                message = "Ticket assigned successfully.",
                ticketId = ticket.TicketId,
                assignedAgentId = agent.UserId,
                assignedAgentName = agent.UserName
            });
        }
        // PUT: api/tickets/{id}/unassign
        [HttpPut("{id}/unassign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnassignTicket(int id)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

            var currentAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == id &&
                    t.TenantId == currentAdmin.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            if (ticket.AssignedAgentId == null)
                return BadRequest("Ticket is already unassigned.");

            var oldAgentId = ticket.AssignedAgentId.Value;

            ticket.AssignedAgentId = null;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

           
            await _notificationService.SendToUserAsync(
                oldAgentId,
                $"Ticket \"{ticket.TicketTitle}\" has been unassigned from you.");

            return Ok("Agent unassigned successfully.");
        }

        // PUT: api/tickets/{id}/status
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateTicketStatus(
            int id,
            UpdateTicketStatusDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");


            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == id &&
                    t.TenantId == currentUser.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");
           
            if (currentUser.Role == UserRole.Customer &&
                ticket.CreatedByUserId != currentUser.UserId)
            {
                return Forbid();
            }

           
            if (currentUser.Role == UserRole.Agent &&
                ticket.AssignedAgentId != currentUser.UserId)
            {
                return Forbid();
            }

      

            if (ticket.Status == dto.Status)
                return BadRequest("Ticket is already in this status.");

            var oldStatus = ticket.Status;

            ticket.Status = dto.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            var history = new TicketStatusHistory
            {
                TicketId = ticket.TicketId,
                OldStatus = oldStatus,
                NewStatus = dto.Status,
                ChangedByUserId = currentUser.UserId,
                ChangedAt = DateTime.UtcNow
            };

            _context.TicketStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            var notificationUserIds = new List<int>();

        
            if (currentUser.Role == UserRole.Customer)
            {
             
                if (ticket.AssignedAgentId.HasValue)
                {
                    notificationUserIds.Add(
                        ticket.AssignedAgentId.Value
                    );
                }

                var adminIds = await _context.Users
                    .Where(u =>
                        u.TenantId == currentUser.TenantId &&
                        u.Role == UserRole.Admin &&
                        u.UserId != currentUser.UserId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                notificationUserIds.AddRange(adminIds);
            }


            else if (currentUser.Role == UserRole.Agent)
            {
             
                if (ticket.CreatedByUserId != currentUser.UserId)
                {
                    notificationUserIds.Add(
                        ticket.CreatedByUserId
                    );
                }

                var adminIds = await _context.Users
                    .Where(u =>
                        u.TenantId == currentUser.TenantId &&
                        u.Role == UserRole.Admin &&
                        u.UserId != currentUser.UserId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                notificationUserIds.AddRange(adminIds);
            }


            else if (currentUser.Role == UserRole.Admin)
            {
              
                if (ticket.CreatedByUserId != currentUser.UserId)
                {
                    notificationUserIds.Add(
                        ticket.CreatedByUserId
                    );
                }

                if (ticket.AssignedAgentId.HasValue)
                {
                    notificationUserIds.Add(
                        ticket.AssignedAgentId.Value
                    );
                }
            }


            if (notificationUserIds.Any())
            {
                await _notificationService.SendToUsersAsync(
                    notificationUserIds.Distinct().ToList(),
                    $"Ticket \"{ticket.TicketTitle}\" status changed from {oldStatus} to {dto.Status}."
                );
            }

            return Ok("Ticket status updated successfully.");
        }
        // POST: api/tickets/{ticketId}/comments
        [HttpPost("{ticketId}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(
            int ticketId,
            CreateCommentDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");

            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Comment message is required.");

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == ticketId &&
                    t.TenantId == currentUser.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            if (currentUser.Role == UserRole.Customer &&
                ticket.CreatedByUserId != currentUser.UserId)
            {
                return Forbid();
            }

            if (currentUser.Role == UserRole.Agent &&
                ticket.AssignedAgentId != currentUser.UserId)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                Message = dto.Message.Trim(),
                TicketId = ticket.TicketId,
                UserId = currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            var notificationUserIds = new List<int>();

           
            if (currentUser.Role == UserRole.Customer)
            {
           
                if (ticket.AssignedAgentId.HasValue &&
                    ticket.AssignedAgentId.Value != currentUser.UserId)
                {
                    notificationUserIds.Add(ticket.AssignedAgentId.Value);
                }

             
                var adminIds = await _context.Users
                    .Where(u =>
                        u.TenantId == currentUser.TenantId &&
                        u.Role == UserRole.Admin &&
                        u.UserId != currentUser.UserId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                notificationUserIds.AddRange(adminIds);
            }

           
            else if (currentUser.Role == UserRole.Agent)
            {
          
                if (ticket.CreatedByUserId != currentUser.UserId)
                {
                    notificationUserIds.Add(ticket.CreatedByUserId);
                }

                
                var adminIds = await _context.Users
                    .Where(u =>
                        u.TenantId == currentUser.TenantId &&
                        u.Role == UserRole.Admin &&
                        u.UserId != currentUser.UserId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                notificationUserIds.AddRange(adminIds);
            }

            
            else if (currentUser.Role == UserRole.Admin)
            {
               
                if (ticket.CreatedByUserId != currentUser.UserId)
                {
                    notificationUserIds.Add(ticket.CreatedByUserId);
                }

               
                if (ticket.AssignedAgentId.HasValue &&
                    ticket.AssignedAgentId.Value != currentUser.UserId)
                {
                    notificationUserIds.Add(ticket.AssignedAgentId.Value);
                }
            }

            if (notificationUserIds.Any())
            {
                await _notificationService.SendToUsersAsync(
                    notificationUserIds.Distinct().ToList(),
                    $"New comment added to ticket \"{ticket.TicketTitle}\"."
                );
            }

            return Ok(new
            {
                message = "Comment added successfully.",
                commentId = comment.CommentId
            });
        }

        // GET: api/tickets/{ticketId}/comments
        [HttpGet("{ticketId}/comments")]
        [Authorize]
        public async Task<IActionResult> GetComments(int ticketId)
        {
  
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

        
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == ticketId &&
                    t.TenantId == currentUser.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            if (currentUser.Role == UserRole.Customer &&
                ticket.CreatedByUserId != currentUser.UserId)
            {
                return Forbid();
            }

            if (currentUser.Role == UserRole.Agent &&
                ticket.AssignedAgentId != currentUser.UserId)
            {
                return Forbid();
            }

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.CommentId,
                    c.Message,
                    c.UserId,
                    UserName = c.User.UserName,
                    Role = c.User.Role.ToString(),
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
           
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int adminId))
                return Unauthorized("Invalid user information.");

         
            var currentAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (currentAdmin == null)
                return Unauthorized("Admin not found.");

          
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketId == id &&
                    t.TenantId == currentAdmin.TenantId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            _context.Tickets.Remove(ticket);

            await _context.SaveChangesAsync();

            return Ok("Ticket deleted successfully.");
        }
        // DELETE: api/tickets/comments/{commentId}
        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
          
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return Unauthorized("User information not found.");

            if (!int.TryParse(userClaim.Value, out int userId))
                return Unauthorized("Invalid user information.");

         
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null)
                return Unauthorized("User not found.");

   
            var comment = await _context.Comments
                .Include(c => c.Ticket)
                .FirstOrDefaultAsync(c =>
                    c.CommentId == commentId &&
                    c.Ticket.TenantId == currentUser.TenantId);

            if (comment == null)
                return NotFound("Comment not found.");

            if (currentUser.Role != UserRole.Admin &&
                comment.UserId != currentUser.UserId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return Ok("Comment deleted successfully.");
        }
    }


}