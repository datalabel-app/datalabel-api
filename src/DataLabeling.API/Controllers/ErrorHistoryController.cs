using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataLabeling.DAL.Data;

namespace DataLabeling.API.Controllers
{
    [Route("api/error-history")]
    [ApiController]
    public class ErrorHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ErrorHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetErrorsByDataset(int datasetId)
        {
            var errors = await _context.TaskErrorHistories
                .Include(e => e.DataItem)
                .Where(e => e.DataItem.DatasetId == datasetId)
                .Select(e => new
                {
                    e.ErrorId,
                    e.TaskId,
                    e.ItemId,
                    e.ErrorMessage,
                    e.ReviewerId,
                    e.CreatedAt,
                    fileUrl = e.DataItem.FileUrl
                })
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(errors);
        }


        [HttpGet("dataset/{datasetId}/group")]
        public async Task<IActionResult> GetErrorsGroupedByItem(int datasetId)
        {
            var result = await _context.TaskErrorHistories
                .Include(e => e.DataItem)
                .Where(e => e.DataItem.DatasetId == datasetId)
                .GroupBy(e => new { e.ItemId, e.DataItem.FileUrl })
                .Select(g => new
                {
                    itemId = g.Key.ItemId,
                    fileUrl = g.Key.FileUrl,
                    errors = g.Select(e => new
                    {
                        errorId = e.ErrorId,
                        errorMessage = e.ErrorMessage,
                        reviewerId = e.ReviewerId,
                        createdAt = e.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(result);
        }


        [HttpGet("dataset/{datasetId}/summary")]
        public async Task<IActionResult> GetErrorSummary(int datasetId)
        {
            var summary = await _context.TaskErrorHistories
                .Include(e => e.DataItem)
                .Where(e => e.DataItem.DatasetId == datasetId)
                .GroupBy(e => e.ErrorMessage)
                .Select(g => new
                {
                    ErrorType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(summary);
        }
    }
}