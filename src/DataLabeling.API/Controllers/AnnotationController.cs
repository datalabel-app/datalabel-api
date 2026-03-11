using DataLabeling.DAL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataLabeling.API.Controllers
{
    [Route("api/annotations")]
    [ApiController]
    public class AnnotationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnotationController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        [HttpPost]
        public async Task<IActionResult> CreateAnnotation([FromBody] Annotation request)
        {
            var item = await _context.DataItems.FindAsync(request.ItemId);
            var label = await _context.Labels.FindAsync(request.LabelId);

            if (item == null)
                return BadRequest("DataItem not found");

            if (label == null)
                return BadRequest("Label not found");

            var annotation = new Annotation
            {
                ItemId = request.ItemId,
                LabelId = request.LabelId,
                RoundId = request.RoundId,
                AnnotatorId = request.AnnotatorId,
                ShapeType = request.ShapeType,
                Coordinates = request.Coordinates,
                Classification = request.Classification,
                CreatedAt = DateTime.UtcNow
            };

            _context.Annotations.Add(annotation);
            await _context.SaveChangesAsync();

            return Ok(annotation);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var annotations = await _context.Annotations
                .Include(a => a.DataItem)
                .Include(a => a.Label)
                .Include(a => a.Annotator)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(annotations);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var annotation = await _context.Annotations
                .Include(a => a.Label)
                .Include(a => a.DataItem)
                .FirstOrDefaultAsync(a => a.AnnotationId == id);

            if (annotation == null)
                return NotFound("Annotation not found");

            return Ok(annotation);
        }


        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetByItem(int itemId)
        {
            var annotations = await _context.Annotations
                .Where(a => a.ItemId == itemId)
                .Include(a => a.Label)
                .ToListAsync();

            return Ok(annotations);
        }


        [HttpGet("annotator/{userId}")]
        public async Task<IActionResult> GetByAnnotator(int userId)
        {
            var annotations = await _context.Annotations
                .Where(a => a.AnnotatorId == userId)
                .Include(a => a.DataItem)
                .Include(a => a.Label)
                .ToListAsync();

            return Ok(annotations);
        }


    }
}