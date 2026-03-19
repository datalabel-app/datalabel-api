using DataLabeling.API.DTOs;
using DataLabeling.DAL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataLabeling.API.Controllers
{
    [Route("api/datasets")]
    [ApiController]
    public class DatasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatasetController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateDataset([FromBody] CreateDatasetRequest request)
        {
            var project = await _context.Projects.FindAsync(request.ProjectId);

            if (project == null)
                return BadRequest("Project not found");

            var dataset = new Dataset
            {
                ProjectId = request.ProjectId,
                DatasetName = request.DatasetName,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Datasets.Add(dataset);
            await _context.SaveChangesAsync();

            var response = new DatasetResponse
            {
                DatasetId = dataset.DatasetId,
                ProjectId = dataset.ProjectId,
                DatasetName = dataset.DatasetName,
                Status = dataset.Status
            };

            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllDatasets()
        {
            var datasets = await _context.Datasets
                .Include(d => d.Project)
                .Include(d => d.Rounds)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(datasets);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataset(int id)
        {
            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .Include(d => d.DataItems)
                .Include(d => d.Rounds)
                .FirstOrDefaultAsync(d => d.DatasetId == id);

            if (dataset == null)
                return NotFound("Dataset not found");

            var result = new DatasetDetailDto
            {
                DatasetId = dataset.DatasetId,
                DatasetName = dataset.DatasetName,

                Status = dataset.Status,

                Project = new ProjectDto
                {
                    ProjectId = dataset.Project.ProjectId,
                    ProjectName = dataset.Project.ProjectName
                },

                

               
            };

            return Ok(result);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetDatasetsByProject(int projectId)
        {
            var datasets = await _context.Datasets
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.Rounds)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(datasets);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataset(int id, UpdateDatasetRequest request)
        {
            var dataset = await _context.Datasets.FindAsync(id);

            if (dataset == null)
                return NotFound("Dataset not found");

            dataset.DatasetName = request.DatasetName;
            dataset.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(dataset);
        }

 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataset(int id)
        {
            var dataset = await _context.Datasets
                .Include(d => d.DataItems)
                .Include(d => d.Rounds)
                .FirstOrDefaultAsync(d => d.DatasetId == id);

            if (dataset == null)
                return NotFound("Dataset not found");

            _context.Datasets.Remove(dataset);
            await _context.SaveChangesAsync();

            return Ok("Dataset deleted");
        }
    }
}