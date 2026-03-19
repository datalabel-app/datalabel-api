using DataLabeling.API.DTOs;
using DataLabeling.DAL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var project = new Project
            {
                ManagerId = managerId,
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
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.Datasets)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var response = projects.Select(p => new ProjectResponseAll
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                ManagerName = p.Manager != null ? p.Manager.FullName : "",
                DatasetCount = p.Datasets != null ? p.Datasets.Count : 0
            });

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.Datasets)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
                return NotFound("Project not found");

            var response = new ProjectResponse
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt,

                Manager = new ManagerResponse
                {
                    UserId = project.Manager.UserId,
                    FullName = project.Manager.FullName,
                    Email = project.Manager.Email
                },

                Datasets = project.Datasets.Select(d => new DatasetResponse
                {
                    DatasetId = d.DatasetId,
                    DatasetName = d.DatasetName,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }


        [HttpGet("manager/{managerId}")]
        public async Task<IActionResult> GetProjectByManager(int managerId)
        {
            var projects = await _context.Projects
                .Where(p => p.ManagerId == managerId)
                .Include(p => p.Datasets)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest request)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound("Project not found");

            project.ProjectName = request.ProjectName;
            project.Description = request.Description;
            project.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Datasets)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
                return NotFound("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok("Project deleted");
        }
    }
}