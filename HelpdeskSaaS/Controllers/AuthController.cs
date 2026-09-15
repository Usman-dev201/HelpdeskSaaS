using HelpdeskSaaS.Data;
using HelpdeskSaaS.DTOs;
using HelpdeskSaaS.Enums;
using HelpdeskSaaS.Models;
using HelpdeskSaaS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSaaS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            PasswordService passwordService,
            JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        // POST: api/Auth/register-tenant
        [HttpPost("register-tenant")]
        public async Task<IActionResult> RegisterTenant(RegisterTenantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenantName) ||
                string.IsNullOrWhiteSpace(dto.UserName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("All fields are required.");
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest("Email already exists.");
            }

            var tenant = new Tenant
            {
                TenantName = dto.TenantName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync();

            var adminUser = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Role = UserRole.Admin,
                TenantId = tenant.TenantId
            };

            _context.Users.Add(adminUser);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tenant registered successfully.",
                tenantId = tenant.TenantId,
                userId = adminUser.UserId,
                role = adminUser.Role.ToString()
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordValid = _passwordService.VerifyPassword(
                dto.Password,
                user.PasswordHash);

            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                role = user.Role.ToString(),
                tenantId = user.TenantId
            });
        }
    }
}