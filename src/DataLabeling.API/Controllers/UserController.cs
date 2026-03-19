using DataLabeling.API.DTOs;
using DataLabeling.BLL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using Task = System.Threading.Tasks.Task;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public UserController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(users);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }


        [HttpGet("annotators")]
        public async Task<IActionResult> GetAnnotators()
        {
            var users = await _context.Users
                .Where(x => x.Role == UserRole.Annotator)
                .ToListAsync();

            return Ok(users);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            bool isMatch = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);

            if (!isMatch)
                return BadRequest(new { message = "Old password is incorrect" });

            if (request.OldPassword == request.NewPassword)
                return BadRequest(new { message = "New password must be different" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.IsChangePassword = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.Status,
                    u.IsChangePassword,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateUserRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;



            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Update success",
                user.UserId,
                user.FullName,
                user.Email,
                user.Role,
                user.Status
            });
        }


        [HttpGet("reviewers")]
        public async Task<IActionResult> GetReviewers()
        {
            var users = await _context.Users
                .Where(x => x.Role == UserRole.Reviewer)
                .ToListAsync();

            return Ok(users);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deleted successfully"
            });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                return NotFound("User not found");

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Status updated"
            });
        }
    }
}
