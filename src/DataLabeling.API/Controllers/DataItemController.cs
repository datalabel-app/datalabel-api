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
    public class DataItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataItemController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateDataItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d =>
                    d.DatasetId == request.DatasetId &&
                    d.Project.ManagerId == userId);

            if (dataset == null)
                return BadRequest("Dataset not found or not yours");

            var entity = new DataItem
            {
                DatasetId = request.DatasetId,
                FileUrl = request.FileUrl,
                Status = request.Status
            };

            _context.DataItems.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(entity));
        }

        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetByDataset(int datasetId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d =>
                    d.DatasetId == datasetId &&
                    d.Project.ManagerId == userId);

            if (dataset == null)
                return BadRequest("Dataset not found or not yours");

            var items = await _context.DataItems
                .Where(i => i.DatasetId == datasetId)
                .Select(i => MapToResponse(i))
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDataItemRequest request)
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

            if (request.FileUrl != null)
                item.FileUrl = request.FileUrl;

            if (request.Status.HasValue)
                item.Status = request.Status.Value;

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(item));
        }

        private static DataItemResponse MapToResponse(DataItem entity)
        {
            return new DataItemResponse
            {
                ItemId = entity.ItemId,
                DatasetId = entity.DatasetId,
                FileUrl = entity.FileUrl,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
