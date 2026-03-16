using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return NotFound();

            return Ok(new
            {
                task.TaskId,
                task.Status,

                ItemId = task.DataItemId,

                FileUrl = task.DataItem.FileUrl,

                Round = new
                {
                    task.Round.RoundId,
                    task.Round.RoundNumber,
                    task.Round.ShapeType,
                    task.Round.Description
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
                .Where(t => t.AnnotatorId == userId)
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

        [HttpGet("my-reviewer-tasks")]
        public async Task<IActionResult> GetMyReviewerTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var tasks = await _context.Tasks
                .Where(t => t.ReviewerId == userId)
                .Select(t => new
                {
                    t.TaskId,
                    t.DataItemId,
                    FileUrl = t.DataItem.FileUrl,
                    t.RoundId,
                    RoundNumber = t.Round.RoundNumber,
                    Status = t.Status.ToString(),
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest dto)
        {
            var task = new Entities.Task
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
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            if (dto.AnnotatorId != null)
                task.AnnotatorId = dto.AnnotatorId;

            if (dto.ReviewerId != null)
                task.ReviewerId = dto.ReviewerId;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<DataLabeling.Entities.TaskStatus>(dto.Status, true, out var status))
                {
                    task.Status = status;

                    if (status == DataLabeling.Entities.TaskStatus.Annotating)
                        task.AnnotatedAt = DateTime.UtcNow;

                    if (status == DataLabeling.Entities.TaskStatus.Approved)
                        task.ReviewedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }



        //    [HttpDelete("{id}")]
        //    public async Task<IActionResult> Delete(int id)
        //    {
        //        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        //        var task = await _context.Set<DataLabeling.Entities.Task>()
        //            .Include(t => t.DatasetRound)
        //            .ThenInclude(r => r.Dataset)
        //            .ThenInclude(d => d.Project)
        //            .FirstOrDefaultAsync(t =>
        //                t.TaskId == id &&
        //                t.DatasetRound.Dataset.Project.ManagerId == userId);

        //        if (task == null)
        //            return NotFound("Task not found");

        //        _context.Remove(task);
        //        await _context.SaveChangesAsync();

        //        return Ok("Deleted successfully");
        //    }

        //    private static TaskResponse MapToResponse(Entities.Task entity)
        //    {
        //        return new TaskResponse
        //        {
        //            TaskId = entity.TaskId,
        //            DatasetRoundId = entity.DatasetRoundId,
        //            AssigneeUserId = entity.AssigneeUserId,
        //            Type = entity.Type,
        //            Status = entity.Status,
        //            GroupNumber = entity.GroupNumber,
        //            ParentTaskId = entity.ParentTaskId,
        //            CreatedAt = entity.CreatedAt,
        //            CompletedAt = entity.CompletedAt
        //        };
        //    }
    }
}
