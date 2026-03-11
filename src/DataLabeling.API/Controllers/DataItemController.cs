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

        [HttpPut("{id}/assign-annotator/{userId}")]
        public async Task<IActionResult> AssignAnnotator(int id, int userId)
        {
            var item = await _context.DataItems.FindAsync(id);

            if (item == null)
                return NotFound("DataItem not found");

            item.AnnotatorId = userId;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpGet("annotator")]
        [Authorize]
        public async Task<IActionResult> GetMyAnnotatorItems()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var items = await _context.DataItems
                .Where(d => d.AnnotatorId == userId)
                .Include(d => d.Annotator)
                .Include(d => d.Reviewer)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var response = items.Select(i => new DataItemResponse
            {
                ItemId = i.ItemId,
                DatasetId = i.DatasetId,
                FileUrl = i.FileUrl,
                AnnotatorId = i.AnnotatorId,
                ReviewerId = i.ReviewerId,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                AnnotatorName = i.Annotator != null ? i.Annotator.FullName : null,
                ReviewerName = i.Reviewer != null ? i.Reviewer.FullName : null
            });

            return Ok(response);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var item = await _context.DataItems.FindAsync(id);

            if (item == null)
                return NotFound("DataItem not found");

            item.Status = status;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}/assign-reviewer/{userId}")]
        public async Task<IActionResult> AssignReviewer(int id, int userId)
        {
            var item = await _context.DataItems.FindAsync(id);

            if (item == null)
                return NotFound("DataItem not found");

            item.ReviewerId = userId;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        [HttpGet("reviewer")]
        [Authorize]
        public async Task<IActionResult> GetMyReviewerItems()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var items = await _context.DataItems
                .Where(d => d.ReviewerId == userId)
                .Include(d => d.Annotator)
                .Include(d => d.Reviewer)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var response = items.Select(i => new DataItemResponse
            {
                ItemId = i.ItemId,
                DatasetId = i.DatasetId,
                FileUrl = i.FileUrl,
                AnnotatorId = i.AnnotatorId,
                ReviewerId = i.ReviewerId,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                AnnotatorName = i.Annotator != null ? i.Annotator.FullName : null,
                ReviewerName = i.Reviewer != null ? i.Reviewer.FullName : null
            });

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataItem(int id)
        {
            var item = await _context.DataItems
                .Include(d => d.Annotations)
                .FirstOrDefaultAsync(d => d.ItemId == id);

            if (item == null)
                return NotFound("DataItem not found");

            _context.DataItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("DataItem deleted");
        }
    }
}
