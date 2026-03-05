using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LabelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LabelController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateLabelRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId && p.ManagerId == userId);

            if (project == null)
                return BadRequest("Project not found or not yours");

            var label = new Label
            {
                ProjectId = request.ProjectId,
                LabelName = request.LabelName,
                LabelType = request.LabelType,
                Description = request.Description
            };

            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var response = new LabelResponse
            {
                LabelId = label.LabelId,
                ProjectId = label.ProjectId,
                LabelName = label.LabelName,
                LabelType = label.LabelType,
                Description = label.Description
            };

            return Ok(response);
        }


        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.ManagerId == userId);

            if (project == null)
                return BadRequest("Project not found or not yours");

            var labels = await _context.Labels
                .Where(l => l.ProjectId == projectId)
                .ToListAsync();

            return Ok(labels);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateLabelRequest request)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var label = await _context.Labels
                .Include(l => l.Project)
                .FirstOrDefaultAsync(l => l.LabelId == id && l.Project.ManagerId == userId);

            if (label == null)
                return NotFound("Label not found");

            label.LabelName = request.LabelName;
            label.LabelType = request.LabelType;
            label.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(label);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var label = await _context.Labels
                .Include(l => l.Project)
                .FirstOrDefaultAsync(l => l.LabelId == id && l.Project.ManagerId == userId);

            if (label == null)
                return NotFound("Label not found");

            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }


    }
}