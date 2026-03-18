using DataLabeling.API.DTOs;
using DataLabeling.API.Hubs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public TaskController(ApplicationDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpGet("round/{roundId}")]
        public async Task<IActionResult> GetTasksByRound(int roundId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.RoundId == roundId && t.Status != Entities.TaskStatus.Pending)
                .Include(t => t.DataItem)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .OrderBy(t => t.TaskId)
                .Select(t => new TaskResponse
                {
                    TaskId = t.TaskId,
                    DataItemId = t.DataItemId,
                    RoundId = t.RoundId,

                    AnnotatorId = t.AnnotatorId,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,

                    ReviewerId = t.ReviewerId,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,

                    Status = t.Status.ToString(),

                    CreatedAt = t.CreatedAt,
                    AnnotatedAt = t.AnnotatedAt,
                    ReviewedAt = t.ReviewedAt,

                    FileUrl = t.DataItem.FileUrl
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
        {
            var tasks = await _context.Tasks
                .Select(t => new TaskResponse
                {
                    TaskId = t.TaskId,
                    DataItemId = t.DataItemId,
                    RoundId = t.RoundId,
                    AnnotatorId = t.AnnotatorId,
                    ReviewerId = t.ReviewerId,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    AnnotatedAt = t.AnnotatedAt,
                    ReviewedAt = t.ReviewedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.DataItem)
                .Include(t => t.Round)
                .Include(t => t.Annotations)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return NotFound();

            var annotation = task.Annotations.FirstOrDefault();

            return Ok(new
            {
                task.TaskId,
                task.Status,
                task.CreatedAt,
                task.AnnotatedAt,
                task.ReviewedAt,
                task.DescriptionError,

                ItemId = task.DataItemId,

                FileUrl = task.DataItem.FileUrl,

                Round = new
                {
                    task.Round.RoundId,
                    task.Round.RoundNumber,
                    task.Round.ShapeType,
                    task.Round.Description
                },

                Annotation = annotation == null ? null : new
                {
                    annotation.AnnotationId,
                    annotation.LabelId,
                    annotation.ShapeType,
                    annotation.Coordinates,
                    annotation.Classification,
                    annotation.AnnotatorId,
                    annotation.CreatedAt
                }
            });
        }

        [HttpGet("my-annotator-tasks")]
        public async Task<IActionResult> GetMyAnnotatorTasks()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var tasks = await _context.Tasks
                .Include(t => t.DataItem)
                    .ThenInclude(d => d.Dataset)
                .Include(t => t.Round)
                .Where(t => t.AnnotatorId == userId)
                 .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TaskId,
                    t.DataItemId,
                    t.Status,
                    t.CreatedAt,

                    FileUrl = t.DataItem.FileUrl,

                    Dataset = new
                    {
                        t.DataItem.Dataset.DatasetId,
                        t.DataItem.Dataset.DatasetName,
                        t.DataItem.Dataset.CreatedAt
                    },

                    Round = new
                    {
                        t.Round.RoundId,
                        t.Round.RoundNumber,
                        t.Round.ShapeType,
                        t.Round.Description,
                        t.Round.Status,
                        t.Round.CreatedAt
                    }
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("my-reviewer-tasks")]
        public async Task<IActionResult> GetMyReviewerTasks()
        {
            var userId = int.Parse(
              User.FindFirst(ClaimTypes.NameIdentifier)!.Value
          );

            var tasks = await _context.Tasks
                .Include(t => t.DataItem)
                .Include(t => t.Round)
                .Where(t => t.ReviewerId == userId)
                 .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TaskId,
                    t.DataItemId,
                    t.Status,
                    t.CreatedAt,

                    FileUrl = t.DataItem.FileUrl,

                    Round = new
                    {
                        t.Round.RoundId,
                        t.Round.RoundNumber,
                        t.Round.ShapeType,
                        t.Round.Description,
                        t.Round.Status,
                        t.Round.CreatedAt,
                    }
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest dto)
        {

            var exists = await _context.Tasks
            .AnyAsync(t => t.DataItemId == dto.DataItemId && t.RoundId == dto.RoundId);

            if (exists)
            {
                return BadRequest("Task for this DataItem and Round already exists.");
            }

            var task = new DataLabeling.Entities.Task
            {
                DataItemId = dto.DataItemId,
                RoundId = dto.RoundId,
                AnnotatorId = dto.AnnotatorId,
                ReviewerId = dto.ReviewerId,
                Status = DataLabeling.Entities.TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = new TaskResponse
            {
                TaskId = task.TaskId,
                DataItemId = task.DataItemId,
                RoundId = task.RoundId,
                AnnotatorId = task.AnnotatorId,
                ReviewerId = task.ReviewerId,
                Status = task.Status.ToString(),
                CreatedAt = task.CreatedAt
            };

            return CreatedAtAction(nameof(GetTaskById), new { taskId = task.TaskId }, result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskRequest dto)
        {
            var task = await _context.Tasks
                .Include(t => t.DataItem)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return NotFound("Task not found");

            if (dto.AnnotatorId != null)
                task.AnnotatorId = dto.AnnotatorId;

            if (dto.ReviewerId != null)
                task.ReviewerId = dto.ReviewerId;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<Entities.TaskStatus>(dto.Status, true, out var status))
                {
                    task.Status = status;

                    switch (status)
                    {
                        case Entities.TaskStatus.Annotating:
                            task.AnnotatedAt = DateTime.UtcNow;

                            if (task.DataItem != null)
                                task.DataItem.Status = "Annotating";
                            break;

                        case Entities.TaskStatus.Approved:
                            task.ReviewedAt = DateTime.UtcNow;

                            if (task.DataItem != null)
                                task.DataItem.Status = "Approved";
                            break;
                        case Entities.TaskStatus.Rejected:
                            task.ReviewedAt = DateTime.UtcNow;

                            if (!string.IsNullOrEmpty(dto.DescriptionError))
                                task.DescriptionError = dto.DescriptionError;

                            if (task.DataItem != null)
                                task.DataItem.Status = "Rejected";

                            var errorHistory = new TaskErrorHistory
                            {
                                TaskId = task.TaskId,
                                ItemId = task.DataItemId,
                                ReviewerId = task.ReviewerId ?? 0,
                                ErrorMessage = dto.DescriptionError ?? "Unknown error",
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TaskErrorHistories.Add(errorHistory);

                            if (task.AnnotatorId != null)
                            {
                                await _hub.Clients
                                    .Group(task.AnnotatorId.ToString())
                                    .SendAsync("ReceiveNotification", new
                                    {
                                        message = "The task has been rejected; please resubmit!",
                                        taskId = task.TaskId,
                                        error = dto.DescriptionError
                                    });
                            }

                            break;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                task.TaskId,
                task.Status,
                task.AnnotatedAt,
                task.ReviewedAt,
                task.DescriptionError,
                dataItemStatus = task.DataItem?.Status
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
