using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/export")]
    public class ExportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] int? roundId, [FromQuery] int? datasetId)
        {
            if (!roundId.HasValue && !datasetId.HasValue)
            {
                return BadRequest("roundId or datasetId is required");
            }

            var query = _context.Tasks
                .Where(t => t.Status == DataLabeling.Entities.TaskStatus.Approved)
                .Include(t => t.DataItem)
                .Include(t => t.Annotations)
                    .ThenInclude(a => a.Label)
                .AsQueryable();

            if (roundId.HasValue)
            {
                query = query.Where(t => t.RoundId == roundId.Value);
            }
            else
            {
                query = query.Where(t => t.DataItem.DatasetId == datasetId.Value);
            }

            var result = await query.Select(t => new
            {
                itemId = t.DataItemId,
                image = t.DataItem.FileUrl,
                roundId = t.RoundId,

                annotations = t.Annotations.Select(a => new
                {
                    annotationId = a.AnnotationId,
                    labelId = a.LabelId,
                    labelName = a.Label.LabelName,
                    shapeType = a.ShapeType,
                    coordinates = a.Coordinates,
                    classification = a.Classification
                }).ToList()
            }).ToListAsync();

            return Ok(new
            {
                total = result.Count,
                data = result
            });
        }
    }
}