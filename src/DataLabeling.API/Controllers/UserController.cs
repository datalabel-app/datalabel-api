using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
