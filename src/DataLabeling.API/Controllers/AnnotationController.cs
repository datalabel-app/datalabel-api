using DataLabeling.API.DTOs;
using DataLabeling.API.Hubs;
using DataLabeling.BLL;
using DataLabeling.DAL.Data;
using DataLabeling.DTOs.Annotations;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/annotations")]
[ApiController]
[Authorize]
public class AnnotationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly EmailService _emailService;

    public AnnotationController(ApplicationDbContext context, IHubContext<NotificationHub> hub, EmailService emailService)
    {
        _context = context;
        _hub = hub;
        _emailService = emailService;
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
        if (task.Deadline.HasValue && DateTime.UtcNow > task.Deadline.Value)
            return BadRequest("Task is past deadline");
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

        var totalTaskItems = await _context.TaskDataItems
            .CountAsync(x => x.TaskId == dto.TaskId);
        var annotatedItems = await _context.Annotations
            .Where(x => x.TaskId == dto.TaskId)
            .Select(x => x.ItemId)
            .Distinct()
            .CountAsync();

        if (annotatedItems >= totalTaskItems && task.ReviewerId != null)
        {
            await _hub.Clients
                .Group(task.ReviewerId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    message = "Task has been annotated and ready for review!",
                    taskId = task.TaskId,
                    type = "TASK_READY_FOR_REVIEW"
                });

            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .FirstOrDefaultAsync(r => r.RoundId == dto.RoundId);

            var reviewer = await _context.Users.FindAsync(task.ReviewerId);
            if (reviewer != null && round != null)
            {
                _ = _emailService.SendTaskReadyForReviewEmailAsync(
                    reviewer.Email,
                    reviewer.FullName,
                    task.TaskId,
                    round.Dataset.DatasetName,
                    round.Description ?? ""
                );
            }
        }

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
        var task = await _context.Tasks
           .FirstOrDefaultAsync(x => x.TaskId == request.TaskId);
        var annotationIds = request.Items.Select(i => i.AnnotationId).ToList();

        var annotations = await _context.Annotations
            .Where(a => annotationIds.Contains(a.AnnotationId))
            .ToListAsync();

        var itemIds = annotations.Select(a => a.ItemId).Distinct().ToList();
        var dataItems = await _context.DataItems
            .Where(x => itemIds.Contains(x.ItemId))
            .ToListAsync();

        foreach (var item in request.Items)
        {
            var annotation = annotations
                .FirstOrDefault(a => a.AnnotationId == item.AnnotationId);

            if (annotation == null) continue;

            annotation.LabelId = item.LabelId;
            annotation.Classification = item.Classification;

            var dataItem = dataItems.FirstOrDefault(x => x.ItemId == annotation.ItemId);
            if (dataItem != null)
            {
                dataItem.Status = "Annotated";
            }
        }
        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var totalTaskItems = await _context.TaskDataItems
            .CountAsync(x => x.TaskId == request.TaskId);
        var annotatedItems = await _context.Annotations
            .Where(x => x.TaskId == request.TaskId)
            .Select(x => x.ItemId)
            .Distinct()
            .CountAsync();

        if (annotatedItems >= totalTaskItems && task.ReviewerId != null)
        {
            await _hub.Clients
                .Group(task.ReviewerId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    message = "Task has been annotated and ready for review!",
                    taskId = task.TaskId,
                    type = "TASK_READY_FOR_REVIEW"
                });

            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .FirstOrDefaultAsync(r => r.RoundId == task.RoundId);

            var reviewer = await _context.Users.FindAsync(task.ReviewerId);
            if (reviewer != null && round != null)
            {
                _ = _emailService.SendTaskReadyForReviewEmailAsync(
                    reviewer.Email,
                    reviewer.FullName,
                    task.TaskId,
                    round.Dataset.DatasetName,
                    round.Description ?? ""
                );
            }
        }

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
        if (task.Deadline.HasValue && DateTime.UtcNow > task.Deadline.Value)
            return BadRequest("Task is past deadline");
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

        dataItem.Status = "Annotated";

        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;

        taskDataItem.ReviewStatus = "Annotating";
        taskDataItem.ReviewComment = null;
        taskDataItem.ReviewerId = null;
        taskDataItem.ReviewedAt = null;

        await _context.SaveChangesAsync();

        var totalTaskItems = await _context.TaskDataItems
            .CountAsync(x => x.TaskId == dto.TaskId);
        var annotatedItems = await _context.Annotations
            .Where(x => x.TaskId == dto.TaskId)
            .Select(x => x.ItemId)
            .Distinct()
            .CountAsync();

        if (annotatedItems >= totalTaskItems && task.ReviewerId != null)
        {
            await _hub.Clients
                .Group(task.ReviewerId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    message = "Task has been annotated and ready for review!",
                    taskId = task.TaskId,
                    type = "TASK_READY_FOR_REVIEW"
                });

            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .FirstOrDefaultAsync(r => r.RoundId == dto.RoundId);

            var reviewer = await _context.Users.FindAsync(task.ReviewerId);
            if (reviewer != null && round != null)
            {
                _ = _emailService.SendTaskReadyForReviewEmailAsync(
                    reviewer.Email,
                    reviewer.FullName,
                    task.TaskId,
                    round.Dataset.DatasetName,
                    round.Description ?? ""
                );
            }
        }

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

        var itemId = annotation.ItemId;

        _context.Annotations.Remove(annotation);
        await _context.SaveChangesAsync();

        var remainingAnnotations = await _context.Annotations
            .AnyAsync(a => a.ItemId == itemId);

        if (!remainingAnnotations)
        {
            var dataItem = await _context.DataItems
                .FirstOrDefaultAsync(x => x.ItemId == itemId);

            if (dataItem != null)
            {
                dataItem.Status = "Pending";
                await _context.SaveChangesAsync();
            }
        }

        return NoContent();
    }

}