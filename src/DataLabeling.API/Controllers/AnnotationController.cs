using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.DTOs.Annotations;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/annotations")]
[ApiController]
[Authorize]
public class AnnotationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnnotationController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpPost("classification/bulk")]
    public async Task<IActionResult> BulkCreateClassification(BulkCreateAnnotationDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var task = await _context.Tasks
            .FirstOrDefaultAsync(x => x.TaskId == dto.TaskId);

        if (task == null)
            return BadRequest("Task not found");

        var itemIds = dto.Items.Select(x => x.ItemId).ToList();

        var dataItems = await _context.DataItems
            .Where(x => itemIds.Contains(x.ItemId))
            .ToListAsync();

        if (dataItems.Count != itemIds.Count)
            return BadRequest("Some DataItems not found");

        var annotations = new List<Annotation>();

        foreach (var item in dto.Items)
        {
            var annotation = new Annotation
            {
                TaskId = dto.TaskId,
                ItemId = item.ItemId,
                LabelId = item.LabelId,
                RoundId = dto.RoundId,
                AnnotatorId = userId,
                ShapeType = "classification",
                Coordinates = "",
                Classification = item.Classification ?? item.LabelId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            annotations.Add(annotation);

            var dataItem = dataItems.First(x => x.ItemId == item.ItemId);
            dataItem.Status = "Annotated";
        }

        _context.Annotations.AddRange(annotations);

        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bulk classification created",
            total = annotations.Count
        });
    }

    [Authorize]
    [HttpPut("bulk-update")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateAnnotationDto request)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest("No items to update");

        var annotationIds = request.Items.Select(i => i.AnnotationId).ToList();

        var annotations = await _context.Annotations
            .Where(a => annotationIds.Contains(a.AnnotationId))
            .ToListAsync();

        foreach (var item in request.Items)
        {
            var annotation = annotations
                .FirstOrDefault(a => a.AnnotationId == item.AnnotationId);

            if (annotation == null) continue;

            annotation.LabelId = item.LabelId;
            annotation.Classification = item.Classification;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bulk update success",
            total = request.Items.Count
        });
    }

    [HttpPost("shape")]
    public async Task<ActionResult<AnnotationResponse>> CreateShapeAnnotation(CreateAnnotationRequest dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("User not found in token");

        int annotatorId = int.Parse(userIdClaim.Value);

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);

        if (task == null)
            return BadRequest("Task not found");

        if (task.AnnotatorId != annotatorId)
            return Forbid("You are not assigned to this task");

        var dataItem = await _context.DataItems
            .FirstOrDefaultAsync(x => x.ItemId == dto.ItemId);

        if (dataItem == null)
            return BadRequest("DataItem not found");

        var taskDataItem = await _context.TaskDataItems
            .FirstOrDefaultAsync(x =>
                x.TaskId == dto.TaskId && x.DataItemId == dto.ItemId);

        if (taskDataItem == null)
            return BadRequest("TaskDataItem not found");

        var annotation = new Annotation
        {
            TaskId = dto.TaskId,
            ItemId = dto.ItemId,
            LabelId = dto.LabelId,
            RoundId = dto.RoundId,
            AnnotatorId = annotatorId,
            ShapeType = dto.ShapeType,
            Coordinates = dto.Coordinates,
            Classification = dto.Classification,
            CreatedAt = DateTime.UtcNow
        };

        _context.Annotations.Add(annotation);

        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;

        taskDataItem.ReviewStatus = "Annotating";
        taskDataItem.ReviewComment = null;
        taskDataItem.ReviewerId = null;
        taskDataItem.ReviewedAt = null;

        await _context.SaveChangesAsync();

        return Ok(new AnnotationResponse
        {
            AnnotationId = annotation.AnnotationId,
            TaskId = annotation.TaskId,
            LabelId = annotation.LabelId,
            ShapeType = annotation.ShapeType,
            Coordinates = annotation.Coordinates,
            Classification = annotation.Classification,
            CreatedAt = annotation.CreatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAnnotations(
       [FromQuery] int? taskId,
       [FromQuery] int? itemId
   )
    {
        var query = _context.Annotations
            .Include(a => a.Label)
            .AsQueryable();

        if (taskId.HasValue)
        {
            query = query.Where(a => a.TaskId == taskId.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(a => a.ItemId == itemId.Value);
        }

        var data = await query
            .Select(a => new
            {
                annotationId = a.AnnotationId,
                itemId = a.ItemId,
                taskId = a.TaskId,
                roundId = a.RoundId,
                labelId = a.LabelId,
                labelName = a.Label.LabelName,
                shapeType = a.ShapeType,
                coordinates = a.Coordinates,
                classification = a.Classification
            })
            .ToListAsync();

        return Ok(data);
    }

}