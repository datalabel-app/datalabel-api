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
    public class DatasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatasetController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateDatasetRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId && p.ManagerId == userId);

            if (project == null)
                return BadRequest("Project not found or not yours");

            var dataset = new Dataset
            {
                ProjectId = request.ProjectId,
                DatasetName = request.DatasetName,
                Status = request.Status ?? "Active"
            };

            _context.Datasets.Add(dataset);
            await _context.SaveChangesAsync();

            return Ok(new DatasetResponse
            {
                DatasetId = dataset.DatasetId,
                ProjectId = dataset.ProjectId,
                DatasetName = dataset.DatasetName,
                Status = dataset.Status,
                CreatedAt = dataset.CreatedAt
            });
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.ManagerId == userId);

            if (project == null)
                return BadRequest("Project not found or not yours");

            var datasets = await _context.Datasets
                .Where(d => d.ProjectId == projectId)
                .Select(d => new DatasetResponse
                {
                    DatasetId = d.DatasetId,
                    ProjectId = d.ProjectId,
                    DatasetName = d.DatasetName,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(datasets);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDatasetRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.DatasetId == id && d.Project.ManagerId == userId);

            if (dataset == null)
                return NotFound("Dataset not found");

            dataset.DatasetName = request.DatasetName;
            dataset.Status = request.Status ?? dataset.Status;

            await _context.SaveChangesAsync();

            return Ok("Updated successfully");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.DatasetId == id && d.Project.ManagerId == userId);

            if (dataset == null)
                return NotFound("Dataset not found");

            _context.Datasets.Remove(dataset);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
