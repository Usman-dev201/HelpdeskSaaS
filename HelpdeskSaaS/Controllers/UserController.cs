
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
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;

        public UsersController(
            ApplicationDbContext context,
            PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
          
            var tenantId = GetCurrentTenantId();

            if (tenantId == null)
                return Unauthorized("Tenant information not found.");

            
            if (dto.Role == UserRole.Admin)
                return BadRequest("Admin user cannot be created.");

         
            if (string.IsNullOrWhiteSpace(dto.UserName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("UserName, Email and Password are required.");
            }

           
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                return BadRequest("Email already exists.");

      
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Role = dto.Role,
                TenantId = tenantId.Value
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully.",
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                role = user.Role.ToString(),
                tenantId = user.TenantId
            });
        }


        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var tenantId = GetCurrentTenantId();

            if (tenantId == null)
                return Unauthorized("Tenant information not found.");

            var users = await _context.Users
                .Where(u => u.TenantId == tenantId.Value)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    Role = u.Role.ToString(),

                    TenantName = u.Tenant != null
                        ? u.Tenant.TenantName
                        : null
                })
                .ToListAsync();

            return Ok(users);
        }


        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var tenantId = GetCurrentTenantId();

            if (tenantId == null)
                return Unauthorized("Tenant information not found.");

            var user = await _context.Users
                .Where(u => u.UserId == id &&
                            u.TenantId == tenantId.Value)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.TenantId
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }


        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            CreateUserDto dto)
        {
            var tenantId = GetCurrentTenantId();

            if (tenantId == null)
                return Unauthorized("Tenant information not found.");

         
            if (dto.Role == UserRole.Admin)
                return BadRequest("User cannot be assigned the Admin role.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == id &&
                    u.TenantId == tenantId.Value);

            if (user == null)
                return NotFound("User not found.");

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == dto.Email &&
                    u.UserId != id);

            if (emailExists)
                return BadRequest("Email already exists.");

            if (string.IsNullOrWhiteSpace(dto.UserName) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("UserName and Email are required.");
            }

            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.Role = dto.Role;

       
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash =
                    _passwordService.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully.",
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                role = user.Role.ToString(),
                tenantId = user.TenantId
            });
        }


        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tenantId = GetCurrentTenantId();
            var currentUserId = GetCurrentUserId();

            if (tenantId == null || currentUserId == null)
                return Unauthorized("User information not found.");

            if (id == currentUserId.Value)
                return BadRequest("Admin cannot delete himself.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == id &&
                    u.TenantId == tenantId.Value);

            if (user == null)
                return NotFound("User not found.");

   
            if (user.Role == UserRole.Admin)
                return BadRequest("Admin user cannot be deleted.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }


        private int? GetCurrentTenantId()
        {
            var tenantClaim = User.FindFirst("tenantId");

            if (tenantClaim == null)
                return null;

            if (int.TryParse(tenantClaim.Value, out int tenantId))
                return tenantId;

            return null;
        }


 
        private int? GetCurrentUserId()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null)
                return null;

            if (int.TryParse(userClaim.Value, out int userId))
                return userId;

            return null;
        }
    }
}

