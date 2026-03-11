using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/dataitems")]
    [ApiController]
    public class DataItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataItemController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateDataItem([FromBody] CreateDataItemRequest request)
        {
            var dataset = await _context.Datasets.FindAsync(request.DatasetId);

            if (dataset == null)
                return BadRequest("Dataset not found");

            var item = new DataItem
            {
                DatasetId = request.DatasetId,
                FileUrl = request.FileUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataItems.Add(item);
            await _context.SaveChangesAsync();

            var response = new DataItemResponse
            {
                ItemId = item.ItemId,
                DatasetId = item.DatasetId,
                FileUrl = item.FileUrl,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDataItems()
        {
            var items = await _context.DataItems
                .Include(d => d.Dataset)
                .Include(d => d.Annotator)
                .Include(d => d.Reviewer)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataItem(int id)
        {
            var item = await _context.DataItems
                .Include(d => d.Dataset)
                .Include(d => d.Annotator)
                .Include(d => d.Reviewer)
                .Include(d => d.Annotations)
                .FirstOrDefaultAsync(d => d.ItemId == id);

            if (item == null)
                return NotFound("DataItem not found");

            return Ok(item);
        }

        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetByDataset(int datasetId)
        {
            var items = await _context.DataItems
                .Where(d => d.DatasetId == datasetId)
                .Include(d => d.Annotator)
                .Include(d => d.Reviewer)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var item = await _context.DataItems
                .Include(i => i.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(i =>
                    i.ItemId == id &&
                    i.Dataset.Project.ManagerId == userId);

            if (item == null)
                return NotFound("DataItem not found");

            _context.DataItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
