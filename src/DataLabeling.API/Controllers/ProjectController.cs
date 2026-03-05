using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectRequest request)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var project = new Project
            {
                ManagerId = userId,
                ProjectName = request.ProjectName,
                Description = request.Description,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(project);
        }


        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var projects = await _context.Projects
                .Where(p => p.ManagerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.ManagerId == userId);

            if (project == null)
                return NotFound("Project not found");

            return Ok(project);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProjectRequest request)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.ManagerId == userId);

            if (project == null)
                return NotFound("Project not found");

            project.ProjectName = request.ProjectName;
            project.Description = request.Description;
            project.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.ManagerId == userId);

            if (project == null)
                return NotFound("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
