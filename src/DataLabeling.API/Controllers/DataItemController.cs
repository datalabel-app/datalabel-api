using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("dataset/{datasetId}/unassigned")]
        public async Task<IActionResult> GetUnassignedByDataset(int datasetId)
        {
            var items = await _context.DataItems
                .Where(d => d.DatasetId == datasetId
                    && !d.TaskDataItems.Any())
                .OrderBy(d => d.ItemId)
                .Select(d => new DataItemResponse
                {
                    ItemId = d.ItemId,
                    DatasetId = d.DatasetId,
                    FileUrl = d.FileUrl,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetByDataset(int datasetId, int? labelId = null)
        {
            var query = _context.DataItems
                .Where(d => d.DatasetId == datasetId)
                .AsQueryable();

            if (labelId.HasValue)
            {
                query = query.Where(d => d.Annotations.Any(a => a.LabelId == labelId.Value));
            }

            var items = await query
                .OrderBy(d => d.ItemId)
                .Select(d => new DataItemResponse
                {
                    ItemId = d.ItemId,
                    DatasetId = d.DatasetId,
                    FileUrl = d.FileUrl,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataItemResponse>>> GetAll()
        {
            var items = await _context.DataItems
                .Select(x => new DataItemResponse
                {
                    ItemId = x.ItemId,
                    DatasetId = x.DatasetId,
                    FileUrl = x.FileUrl,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataItemResponse>> GetById(int id)
        {
            var item = await _context.DataItems
                .Where(x => x.ItemId == id)
                .Select(x => new DataItemResponse
                {
                    ItemId = x.ItemId,
                    DatasetId = x.DatasetId,
                    FileUrl = x.FileUrl,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(item);
        }


        [HttpPost]
        public async Task<ActionResult<DataItemResponse>> Create(CreateDataItemRequest dto)
        {
            var item = new DataItem
            {
                DatasetId = dto.DatasetId,
                FileUrl = dto.FileUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataItems.Add(item);
            await _context.SaveChangesAsync();

            var result = new DataItemResponse
            {
                ItemId = item.ItemId,
                DatasetId = item.DatasetId,
                FileUrl = item.FileUrl,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = item.ItemId }, result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DataItems.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.DataItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
