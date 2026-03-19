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


    [HttpPost("classification")]
    public async Task<IActionResult> CreateClassification(CreateAnnotationDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var task = await _context.Tasks
            .FirstOrDefaultAsync(x => x.TaskId == dto.TaskId);

        if (task == null)
            return BadRequest("Task not found");

        var dataItem = await _context.DataItems
            .FirstOrDefaultAsync(x => x.ItemId == task.DataItemId);

        if (dataItem == null)
            return BadRequest("DataItem not found");

        var annotation = new Annotation
        {
            TaskId = task.TaskId,
            ItemId = task.DataItemId,
            LabelId = dto.LabelId,
            RoundId = dto.RoundId,
            AnnotatorId = userId,
            ShapeType = "classification",
            Coordinates = "",
            Classification = dto.Classification,
            CreatedAt = DateTime.UtcNow
        };

        _context.Annotations.Add(annotation);


        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;


        dataItem.Status = "Pending";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Classification created",
            annotationId = annotation.AnnotationId,
            taskStatus = task.Status,
            dataItemStatus = dataItem.Status
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


        dataItem.Status = "Pending";

        await _context.SaveChangesAsync();

        var result = new AnnotationResponse
        {
            AnnotationId = annotation.AnnotationId,
            TaskId = annotation.TaskId,
            LabelId = annotation.LabelId,
            ShapeType = annotation.ShapeType,
            Coordinates = annotation.Coordinates,
            Classification = annotation.Classification,
            CreatedAt = annotation.CreatedAt
        };

        return Ok(result);
    }


    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetAnnotationsByTask(int taskId)
    {
        var annotations = await _context.Annotations
            .Include(a => a.Label)
            .Include(a => a.Annotator)
            .Where(a => a.TaskId == taskId)
            .Select(a => new
            {
                a.AnnotationId,
                a.TaskId,
                a.ItemId,
                a.LabelId,
                LabelName = a.Label.LabelName,
                a.RoundId,
                a.ShapeType,
                a.Coordinates,
                a.Classification,
                a.AnnotatorId,
                AnnotatorName = a.Annotator.FullName,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(annotations);
    }

    [HttpPut("{annotationId}")]
    public async Task<IActionResult> UpdateAnnotation(int annotationId, UpdateAnnotationRequest dto)
    {
        var annotation = await _context.Annotations
            .Include(a => a.Task)
            .FirstOrDefaultAsync(a => a.AnnotationId == annotationId);

        if (annotation == null)
            return NotFound("Annotation not found");

        var dataItem = await _context.DataItems
            .FirstOrDefaultAsync(x => x.ItemId == annotation.ItemId);


        if (dto.LabelId.HasValue)
            annotation.LabelId = dto.LabelId.Value;

        if (!string.IsNullOrEmpty(dto.Coordinates))
            annotation.Coordinates = dto.Coordinates;

        if (!string.IsNullOrEmpty(dto.Classification))
            annotation.Classification = dto.Classification;

        annotation.CreatedAt = DateTime.UtcNow;


        if (annotation.Task != null)
        {
            annotation.Task.Status = DataLabeling.Entities.TaskStatus.Annotating;

            annotation.Task.AnnotatedAt = DateTime.UtcNow;

            annotation.Task.ReviewedAt = null;
            annotation.Task.DescriptionError = null;
        }


        if (dataItem != null)
        {
            dataItem.Status = "Annotating";
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            annotation.AnnotationId,
            annotation.LabelId,
            annotation.Coordinates,
            annotation.Classification,
            annotation.CreatedAt,

            taskStatus = annotation.Task?.Status,
            dataItemStatus = dataItem?.Status
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnnotation(int id)
    {
        var annotation = await _context.Annotations.FindAsync(id);

        if (annotation == null)
            return NotFound();

        _context.Annotations.Remove(annotation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}