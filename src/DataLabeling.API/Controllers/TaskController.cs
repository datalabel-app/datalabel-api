using DataLabeling.API.DTOs;
using DataLabeling.API.Hubs;
using DataLabeling.DAL.Data;
using DataLabeling.DTOs.Annotations;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var task = new DataLabeling.Entities.Task
                {
                    RoundId = dto.RoundId,
                    AnnotatorId = dto.AnnotatorId,
                    ReviewerId = dto.ReviewerId,
                    Status = DataLabeling.Entities.TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                if (dto.DataItemIds.Any())
                {
                    var taskDataItems = dto.DataItemIds.Select(id => new TaskDataItem
                    {
                        TaskId = task.TaskId,
                        DataItemId = id
                    });

                    _context.TaskDataItems.AddRange(taskDataItems);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                var response = new TaskResponse
                {
                    TaskId = task.TaskId,
                    RoundId = task.RoundId,
                    AnnotatorId = task.AnnotatorId,
                    ReviewerId = task.ReviewerId,
                    Status = task.Status.ToString(),
                    CreatedAt = task.CreatedAt,
                    DataItemIds = dto.DataItemIds
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{taskId}/review")]
        public async Task<IActionResult> GetReviewTask(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskDataItems)
                    .ThenInclude(td => td.DataItem)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return NotFound("Task not found");

            var annotations = await _context.Annotations
                .Where(a => a.TaskId == taskId)
                .Include(a => a.Label)
                .Include(a => a.Annotator)
                .ToListAsync();

            var annotationDict = annotations
                .GroupBy(a => a.ItemId)
                .ToDictionary(g => g.Key, g => g.First());

            var result = new ReviewTaskResponseDto
            {
                TaskId = task.TaskId,
                RoundId = task.RoundId,
                Status = task.Status.ToString(),

                Items = task.TaskDataItems.Select(td =>
                {
                    annotationDict.TryGetValue(td.DataItemId, out var ann);

                    return new ReviewItemDto
                    {
                        ItemId = td.DataItem.ItemId,
                        FileUrl = td.DataItem.FileUrl,

                        ReviewStatus = td.ReviewStatus,
                        ReviewComment = td.ReviewComment,

                        Annotation = ann == null ? null : new AnnotationDto
                        {
                            AnnotationId = ann.AnnotationId,
                            LabelId = ann.LabelId,
                            LabelName = ann.Label.LabelName,

                            AnnotatorId = ann.AnnotatorId,
                            AnnotatorName = ann.Annotator.FullName,

                            CreatedAt = ann.CreatedAt
                        }
                    };
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost("review/bulk")]
        public async Task<IActionResult> BulkReview([FromBody] BulkReviewDto dto)
        {
            var reviewerId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var task = await _context.Tasks
                .Include(t => t.Round)
                .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);

            if (task == null)
                return BadRequest("Task not found");

            var itemIds = dto.Items.Keys.ToList();

            var taskItems = await _context.TaskDataItems
                .Where(x => x.TaskId == dto.TaskId && itemIds.Contains(x.DataItemId))
                .ToListAsync();

            if (taskItems.Count != itemIds.Count)
                return BadRequest("Some items not found in task");

            var errorHistories = new List<TaskErrorHistory>();

            foreach (var taskItem in taskItems)
            {
                var reviewData = dto.Items[taskItem.DataItemId];
                var status = reviewData.Status?.Trim().ToLower();

                if (status != "approved" && status != "rejected")
                    return BadRequest($"Invalid status at item {taskItem.DataItemId}");

                if (taskItem.ReviewStatus == "Approved" || taskItem.ReviewStatus == "Rejected")
                    continue;

                taskItem.ReviewerId = reviewerId;
                taskItem.ReviewedAt = DateTime.UtcNow;

                if (status == "approved")
                {
                    taskItem.ReviewStatus = "Approved";
                    taskItem.ReviewComment = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(reviewData.Comment))
                        return BadRequest($"Item {taskItem.DataItemId} cần comment khi reject");

                    taskItem.ReviewStatus = "Rejected";
                    taskItem.ReviewComment = reviewData.Comment;

                    errorHistories.Add(new TaskErrorHistory
                    {
                        TaskId = dto.TaskId,
                        ItemId = taskItem.DataItemId,
                        ReviewerId = reviewerId,
                        ErrorMessage = reviewData.Comment!,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (errorHistories.Any())
                _context.TaskErrorHistories.AddRange(errorHistories);

            var allItems = await _context.TaskDataItems
                .Where(x => x.TaskId == dto.TaskId)
                .ToListAsync();

            var allApproved = allItems.All(x => x.ReviewStatus == "Approved");
            var hasRejected = allItems.Any(x => x.ReviewStatus == "Rejected");

            if (allApproved)
            {
                task.Status = DataLabeling.Entities.TaskStatus.Done;
                task.ReviewedAt = DateTime.UtcNow;

                await CreateSubDatasetFromLabel(task);
            }
            else if (hasRejected)
            {
                task.Status = DataLabeling.Entities.TaskStatus.Pending;
                task.ReviewedAt = DateTime.UtcNow;
            }
            else
            {
                task.Status = DataLabeling.Entities.TaskStatus.Pending;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Review submitted",
                total = taskItems.Count,
                approved = taskItems.Count(x => x.ReviewStatus == "Approved"),
                rejected = taskItems.Count(x => x.ReviewStatus == "Rejected")
            });
        }

        //[HttpGet("round/{roundId}")]
        //public async Task<IActionResult> GetTasksByRound(int roundId)
        //{
        //    var tasks = await _context.Tasks
        //        .Where(t => t.RoundId == roundId)
        //        .Include(t => t.DataItem)
        //        .Include(t => t.Annotator)
        //        .Include(t => t.Reviewer)
        //        .Include(t => t.Annotations)
        //            .ThenInclude(a => a.Label)
        //        .OrderBy(t => t.TaskId)
        //        .Select(t => new
        //        {
        //            TaskId = t.TaskId,
        //            DataItemId = t.DataItemId,
        //            RoundId = t.RoundId,
        //            AnnotatorId = t.AnnotatorId,
        //            AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
        //            ReviewerId = t.ReviewerId,
        //            ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,
        //            Status = t.Status.ToString(),
        //            CreatedAt = t.CreatedAt,
        //            AnnotatedAt = t.AnnotatedAt,
        //            ReviewedAt = t.ReviewedAt,
        //            FileUrl = t.DataItem.FileUrl,

        //            // Danh sách label gắn với task
        //            Labels = t.Annotations.Select(a => new
        //            {
        //                LabelId = a.LabelId,
        //                LabelName = a.Label.LabelName
        //            }).ToList()
        //        })
        //        .ToListAsync();

        //    return Ok(tasks);
        //}

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
        //{
        //    var tasks = await _context.Tasks
        //        .Select(t => new TaskResponse
        //        {
        //            TaskId = t.TaskId,
        //            DataItemId = t.DataItemId,
        //            RoundId = t.RoundId,
        //            AnnotatorId = t.AnnotatorId,
        //            ReviewerId = t.ReviewerId,
        //            Status = t.Status.ToString(),
        //            CreatedAt = t.CreatedAt,
        //            AnnotatedAt = t.AnnotatedAt,
        //            ReviewedAt = t.ReviewedAt
        //        })
        //        .ToListAsync();

        //    return Ok(tasks);
        //}

        [Authorize]
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var task = await _context.Tasks
                .Where(t => t.TaskId == taskId)
                .Select(t => new TaskDetailDto
                {
                    TaskId = t.TaskId,
                    RoundName = t.Round.Description,
                    RoundId = t.Round.RoundId,
                    ShapeType = (int)t.Round.ShapeType,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,

                    DataItems = t.TaskDataItems
                   .Select(td => new
                   {
                       td,
                       annotations = _context.Annotations
                        .Where(a => a.TaskId == t.TaskId && a.ItemId == td.DataItemId)
                        .Select(a => new AnnotationResponse
                        {
                            AnnotationId = a.AnnotationId,
                            LabelId = a.LabelId,
                            TaskId = a.TaskId,
                            ShapeType = a.ShapeType,
                            Coordinates = a.Coordinates,
                            Classification = a.Classification
                        })
                        .ToList()
                   })
                .Select(x => new TaskDataItemDto
                {
                    ItemId = x.td.DataItem.ItemId,
                    FileUrl = x.td.DataItem.FileUrl,
                    Status = x.td.DataItem.Status,

                    ReviewStatus = x.td.ReviewStatus,
                    ReviewComment = x.td.ReviewComment,

                    Annotations = x.annotations,

                    ErrorMessage = _context.TaskErrorHistories
                        .Where(e => e.TaskId == t.TaskId && e.ItemId == x.td.DataItemId)
                        .OrderByDescending(e => e.CreatedAt)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault()
                })
                .ToList()
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.AnnotatorId = dto.AnnotatorId ?? task.AnnotatorId;
            task.ReviewerId = dto.ReviewerId ?? task.ReviewerId;
            task.Status = dto.Status ?? task.Status;
            task.DescriptionError = dto.DescriptionError ?? task.DescriptionError;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [Authorize]
        [HttpGet("annotator/me")]
        public async Task<IActionResult> GetMyAnnotatorTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var tasks = await _context.Tasks
                .Where(t => t.AnnotatorId == userId)
                .Include(t => t.Round)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .Include(t => t.TaskDataItems)
                .Select(t => new TaskResponseDto
                {
                    TaskId = t.TaskId,
                    RoundName = t.Round.Description,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,
                    DataItemCount = t.TaskDataItems.Count,
                    ShapeType = (int)t.Round.ShapeType,
                    Status = t.Status,
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [Authorize]
        [HttpGet("reviewer/me")]
        public async Task<IActionResult> GetMyReviewerTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var tasks = await _context.Tasks
                .Where(t => t.ReviewerId == userId)
                .Include(t => t.Round)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .Include(t => t.TaskDataItems)
                .Select(t => new TaskResponseDto
                {
                    TaskId = t.TaskId,
                    RoundName = t.Round.Description,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,
                    DataItemCount = t.TaskDataItems.Count,
                    ShapeType = (int)t.Round.ShapeType,
                    Status = t.Status,
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskDataItems)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null) return NotFound();

            _context.TaskDataItems.RemoveRange(task.TaskDataItems);
            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        private async System.Threading.Tasks.Task CreateSubDatasetFromLabel(DataLabeling.Entities.Task task)
        {
            //lấy dataset cha (root dataset)
            var parentDataset = await _context.Datasets
                .FirstOrDefaultAsync(d => d.DatasetId == task.Round.DatasetId);

            if (parentDataset == null)
                return;

            //lấy annotation
            var annotations = await _context.Annotations
                .Include(a => a.Label)
                .Where(a => a.TaskId == task.TaskId)
                .ToListAsync();

            //group theo label
            var groups = annotations
                .GroupBy(a => a.Label.LabelName)
                .ToList();

            foreach (var group in groups)
            {
                var labelName = group.Key;

                //check đã có dataset con chưa
                var existingDataset = await _context.Datasets.FirstOrDefaultAsync(d =>
                    d.ParentDatasetId == parentDataset.DatasetId &&
                    d.DatasetName == labelName
                );

                if (existingDataset == null)
                {
                    var newDataset = new Dataset
                    {
                        DatasetName = labelName,
                        ParentDatasetId = parentDataset.DatasetId,
                        ProjectId = parentDataset.ProjectId,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Datasets.Add(newDataset);
                    await _context.SaveChangesAsync();

                    existingDataset = newDataset;
                }

                //OPTIONAL: add DataItem vào dataset con
                var itemIds = group.Select(x => x.ItemId).Distinct().ToList();

                var dataItems = await _context.DataItems
                    .Where(x => itemIds.Contains(x.ItemId))
                    .ToListAsync();

                foreach (var item in dataItems)
                {
                    // tránh duplicate
                    if (!_context.DataItems.Any(x => x.ItemId == item.ItemId && x.DatasetId == existingDataset.DatasetId))
                    {
                        var newItem = new DataItem
                        {
                            FileUrl = item.FileUrl,
                            DatasetId = existingDataset.DatasetId
                        };

                        _context.DataItems.Add(newItem);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}
