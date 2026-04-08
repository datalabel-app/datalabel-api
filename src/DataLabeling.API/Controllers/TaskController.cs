using DataLabeling.API.DTOs;
using DataLabeling.API.Hubs;
using DataLabeling.BLL;
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
        private readonly EmailService _emailService;

        public TaskController(ApplicationDbContext context, IHubContext<NotificationHub> hub, EmailService emailService)
        {
            _context = context;
            _hub = hub;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            if (dto.Deadline.HasValue && dto.Deadline < DateTime.UtcNow)
            {
                return BadRequest("The deadline must not be earlier than the current time.");
            }
            try
            {
                var task = new DataLabeling.Entities.Task
                {
                    RoundId = dto.RoundId,
                    AnnotatorId = dto.AnnotatorId,
                    ReviewerId = dto.ReviewerId,
                    Status = DataLabeling.Entities.TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Deadline = dto.Deadline
                };
                var existingItemIds = await _context.TaskDataItems
                        .AnyAsync(tdi => dto.DataItemIds.Contains(tdi.DataItemId));
                if (existingItemIds)
                {
                    return BadRequest("Some items already have tasks.");
                }
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

                var round = await _context.DatasetRounds
                    .Include(r => r.Dataset)
                    .FirstOrDefaultAsync(r => r.RoundId == dto.RoundId);

                if (task.AnnotatorId != null)
                {
                    await _hub.Clients
                        .Group(task.AnnotatorId.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            message = "You have been assigned a new task!",
                            taskId = task.TaskId,
                            type = "TASK_ASSIGNED"
                        });

                    var annotator = await _context.Users.FindAsync(task.AnnotatorId);
                    if (annotator != null && round != null)
                    {
                        _ = _emailService.SendTaskAssignmentEmailAsync(
                            annotator.Email,
                            annotator.FullName,
                            task.TaskId,
                            round.Dataset.DatasetName,
                            round.Description ?? "",
                            "Annotator"
                        );
                    }
                }

                if (task.ReviewerId != null)
                {
                    await _hub.Clients
                        .Group(task.ReviewerId.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            message = "A new task is waiting for your review!",
                            taskId = task.TaskId,
                            type = "TASK_FOR_REVIEW"
                        });

                    var reviewer = await _context.Users.FindAsync(task.ReviewerId);
                    if (reviewer != null && round != null)
                    {
                        _ = _emailService.SendTaskAssignmentEmailAsync(
                            reviewer.Email,
                            reviewer.FullName,
                            task.TaskId,
                            round.Dataset.DatasetName,
                            round.Description ?? "",
                            "Reviewer"
                        );
                    }
                }

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

        [Authorize]
        [HttpGet("manager/tasks")]
        public async Task<IActionResult> GetTasksOfManager()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token");

            var managerId = int.Parse(userIdClaim);

            var tasks = await _context.Tasks
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .Include(t => t.Round)
                    .ThenInclude(r => r.Dataset)
                        .ThenInclude(d => d.Project)
                .Where(t => t.Round.Dataset.Project.ManagerId == managerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TaskId,
                    t.Status,
                    t.CreatedAt,
                    t.AnnotatedAt,
                    t.ReviewedAt,
                    t.Deadline,

                    Annotator = t.Annotator != null ? new
                    {
                        t.Annotator.UserId,
                        t.Annotator.FullName
                    } : null,

                    Reviewer = t.Reviewer != null ? new
                    {
                        t.Reviewer.UserId,
                        t.Reviewer.FullName
                    } : null,

                    DatasetName = t.Round.Dataset.DatasetName,
                    ProjectName = t.Round.Dataset.Project.ProjectName
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPut("{taskId}/deadline")]
        public async Task<IActionResult> UpdateDeadline(int taskId, [FromBody] UpdateDeadlineDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
                return BadRequest("Task not found");

            if (!dto.NewDeadline.HasValue)
                return BadRequest("Deadline is required");

            if (dto.NewDeadline <= DateTime.UtcNow)
                return BadRequest("Deadline must be in the future");

            var oldDeadline = task.Deadline;

            if (oldDeadline == dto.NewDeadline)
                return Ok(new { message = "No changes" });

            task.Deadline = dto.NewDeadline;

            if (oldDeadline.HasValue)
            {
                if (task.AnnotatorId != null)
                {
                    var annotator = await _context.Users.FindAsync(task.AnnotatorId);
                    if (annotator != null)
                    {
                        annotator.Points -= 10;
                    }
                }

                if (task.ReviewerId != null)
                {
                    var reviewer = await _context.Users.FindAsync(task.ReviewerId);
                    if (reviewer != null)
                    {
                        reviewer.Points -= 10;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deadline updated successfully",
                newDeadline = task.Deadline
            });
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

                if (taskItem.ReviewStatus == "Approved")
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

            await _context.SaveChangesAsync();

            await PushApprovedItemsToDataset(dto, task.Round.DatasetId);

            var allItems = await _context.TaskDataItems
                .Where(x => x.TaskId == dto.TaskId)
                .ToListAsync();

            var allApproved = allItems.All(x => x.ReviewStatus == "Approved");
            var hasRejected = allItems.Any(x => x.ReviewStatus == "Rejected");
            var previousStatus = task.Status;
            if (allApproved)
            {
                task.Status = DataLabeling.Entities.TaskStatus.Done;
                task.ReviewedAt = DateTime.UtcNow;
            }
            else if (hasRejected)
            {
                task.Status = DataLabeling.Entities.TaskStatus.Review;
                task.ReviewedAt = DateTime.UtcNow;
            }
            else
            {
                task.Status = DataLabeling.Entities.TaskStatus.Review;
            }


            if (previousStatus != DataLabeling.Entities.TaskStatus.Done &&
     task.Status == DataLabeling.Entities.TaskStatus.Done)
            {
                var reviewer = await _context.Users.FindAsync(reviewerId);
                if (reviewer != null)
                {
                    reviewer.Points += 5;
                }

                if (task.AnnotatorId != null)
                {
                    var annotator = await _context.Users.FindAsync(task.AnnotatorId);
                    if (annotator != null)
                    {
                        annotator.Points += 5;
                    }
                }
            }
            await _context.SaveChangesAsync();
            if (hasRejected && task.AnnotatorId != null)
            {
                await _hub.Clients
                    .Group(task.AnnotatorId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        message = "The task has been rejected; please resubmit!",
                        taskId = task.TaskId,
                        type = "TASK_REJECTED"
                    });

                var round = await _context.DatasetRounds
                    .Include(r => r.Dataset)
                    .FirstOrDefaultAsync(r => r.RoundId == task.RoundId);

                var annotator = await _context.Users.FindAsync(task.AnnotatorId);
                if (annotator != null && round != null)
                {
                    _ = _emailService.SendTaskRejectedEmailAsync(
                        annotator.Email,
                        annotator.FullName,
                        task.TaskId,
                        round.Dataset.DatasetName,
                        round.Description ?? "",
                        allItems.Count(x => x.ReviewStatus == "Rejected")
                    );
                }
            }

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
                       AnnotationId = x.annotations.FirstOrDefault().AnnotationId,
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

        [Authorize]
        [HttpGet("annotator/me")]
        public async Task<IActionResult> GetMyAnnotatorTasks(
            [FromQuery] int? status,
            [FromQuery] string? search)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.Tasks
                .Where(t => t.AnnotatorId == userId)
                .Include(t => t.Round)
                    .ThenInclude(r => r.Dataset)
                        .ThenInclude(d => d.Project)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .Include(t => t.TaskDataItems)
                .AsQueryable();

            if (status.HasValue)
            {
                var statusEnum = (DataLabeling.Entities.TaskStatus)status.Value;
                query = query.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(t =>
                    (t.Round.Description != null && t.Round.Description.ToLower().Contains(search)) ||
                    (t.Round.Dataset != null && t.Round.Dataset.DatasetName.ToLower().Contains(search)) ||
                    (t.Round.Dataset.Project.ProjectName.ToLower().Contains(search)) ||
                    (t.Annotator != null && t.Annotator.FullName.ToLower().Contains(search)) ||
                    (t.Reviewer != null && t.Reviewer.FullName.ToLower().Contains(search))
                );
            }

            var tasks = await query
                .OrderByDescending(t => t.TaskId)
                .Select(t => new
                {
                    t.TaskId,
                    RoundName = t.Round.Description,
                    DatasetName = t.Round.Dataset.DatasetName,
                    ProjectName = t.Round.Dataset.Project.ProjectName,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,
                    DataItemCount = t.TaskDataItems.Count,
                    ShapeType = (int)t.Round.ShapeType,
                    Status = t.Status,
                    Deadline = t.Deadline
                })
                .ToListAsync();

            var grouped = tasks
                .GroupBy(t => t.ProjectName)
                .Select(g => new
                {
                    ProjectName = g.Key,
                    TotalTasks = g.Count(),
                    Tasks = g.ToList()
                })
                .OrderByDescending(g => g.TotalTasks);

            return Ok(grouped);
        }

        [Authorize]
        [HttpGet("reviewer/me")]
        public async Task<IActionResult> GetMyReviewerTasks([FromQuery] int? status, [FromQuery] string? search)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.Tasks
                .Where(t => t.ReviewerId == userId)
                .Include(t => t.Round)
                    .ThenInclude(r => r.Dataset)
                        .ThenInclude(d => d.Project)
                .Include(t => t.Annotator)
                .Include(t => t.Reviewer)
                .Include(t => t.TaskDataItems)
                .AsQueryable();

            if (status.HasValue)
            {
                var statusEnum = (DataLabeling.Entities.TaskStatus)status.Value;
                query = query.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(t =>
                    (t.Round.Description != null && t.Round.Description.ToLower().Contains(search)) ||
                    (t.Round.Dataset != null && t.Round.Dataset.DatasetName.ToLower().Contains(search)) ||
                    (t.Round.Dataset.Project.ProjectName.ToLower().Contains(search)) ||
                    (t.Annotator != null && t.Annotator.FullName.ToLower().Contains(search)) ||
                    (t.Reviewer != null && t.Reviewer.FullName.ToLower().Contains(search))
                );
            }

            var tasks = await query
                .OrderByDescending(t => t.TaskId)
                .Select(t => new
                {
                    t.TaskId,
                    RoundName = t.Round.Description,
                    DatasetName = t.Round.Dataset.DatasetName,
                    ProjectName = t.Round.Dataset.Project.ProjectName,
                    AnnotatorName = t.Annotator != null ? t.Annotator.FullName : null,
                    ReviewerName = t.Reviewer != null ? t.Reviewer.FullName : null,
                    DataItemCount = t.TaskDataItems.Count,
                    ShapeType = (int)t.Round.ShapeType,
                    Status = t.Status,
                    Deadline = t.Deadline
                })
                .ToListAsync();

            var grouped = tasks
                .GroupBy(t => t.ProjectName)
                .Select(g => new
                {
                    ProjectName = g.Key,
                    Tasks = g.ToList()
                });

            return Ok(grouped);
        }

        private async Task<List<Dataset>> GetAllSubDatasets(int parentId)
        {
            var result = new List<Dataset>();

            var children = await _context.Datasets
                .Where(d => d.ParentDatasetId == parentId)
                .ToListAsync();

            foreach (var child in children)
            {
                result.Add(child);

                var sub = await GetAllSubDatasets(child.DatasetId);
                result.AddRange(sub);
            }

            return result;
        }

        //private async System.Threading.Tasks.Task CreateSubDatasetFromLabel(DataLabeling.Entities.Task task)
        //{
        //    //lấy dataset cha (root dataset)
        //    var parentDataset = await _context.Datasets
        //        .FirstOrDefaultAsync(d => d.DatasetId == task.Round.DatasetId);

        //    if (parentDataset == null)
        //        return;

        //    //lấy annotation
        //    var annotations = await _context.Annotations
        //        .Include(a => a.Label)
        //        .Where(a => a.TaskId == task.TaskId)
        //        .ToListAsync();

        //    //group theo label
        //    var groups = annotations
        //        .GroupBy(a => a.Label.LabelName)
        //        .ToList();

        //    foreach (var group in groups)
        //    {
        //        var labelName = group.Key;

        //        //check đã có dataset con chưa
        //        var existingDataset = await _context.Datasets.FirstOrDefaultAsync(d =>
        //            d.ParentDatasetId == parentDataset.DatasetId &&
        //            d.DatasetName == labelName
        //        );

        //        if (existingDataset == null)
        //        {
        //            var newDataset = new Dataset
        //            {
        //                DatasetName = labelName,
        //                ParentDatasetId = parentDataset.DatasetId,
        //                ProjectId = parentDataset.ProjectId,
        //                Status = "Active",
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            _context.Datasets.Add(newDataset);
        //            await _context.SaveChangesAsync();

        //            existingDataset = newDataset;
        //        }

        //        //OPTIONAL: add DataItem vào dataset con
        //        var itemIds = group.Select(x => x.ItemId).Distinct().ToList();

        //        var dataItems = await _context.DataItems
        //            .Where(x => itemIds.Contains(x.ItemId))
        //            .ToListAsync();

        //        foreach (var item in dataItems)
        //        {
        //            // tránh duplicate
        //            if (!_context.DataItems.Any(x => x.ItemId == item.ItemId && x.DatasetId == existingDataset.DatasetId))
        //            {
        //                var newItem = new DataItem
        //                {
        //                    FileUrl = item.FileUrl,
        //                    DatasetId = existingDataset.DatasetId
        //                };

        //                _context.DataItems.Add(newItem);
        //            }
        //        }
        //    }

        //    await _context.SaveChangesAsync();
        //}

        private async System.Threading.Tasks.Task PushApprovedItemsToDataset(BulkReviewDto dto, int parentDatasetId)
        {
            Console.WriteLine("===== PUSH USING LABEL FROM PAYLOAD =====");

            var itemIds = dto.Items.Keys.ToList();

            var originalItems = await _context.DataItems
                .Where(d => itemIds.Contains(d.ItemId))
                .ToDictionaryAsync(d => d.ItemId, d => d);

            foreach (var kv in dto.Items)
            {
                var itemId = kv.Key;
                var review = kv.Value;

                // ❌ skip nếu không approved
                if (review.Status?.ToLower() != "approved")
                    continue;

                if (review.LabelId == null)
                {
                    Console.WriteLine($"❌ Item {itemId} thiếu labelId");
                    continue;
                }

                if (!originalItems.ContainsKey(itemId))
                    continue;

                var originalItem = originalItems[itemId];

                // 🔥 TÌM DATASET THEO LABEL
                var dataset = await _context.Datasets
                    .FirstOrDefaultAsync(d =>
                        d.LabelId == review.LabelId &&
                        d.ParentDatasetId != null // optional: đảm bảo là sub dataset
                    );

                // ❗ fallback nếu chưa có dataset gắn label
                if (dataset == null)
                {
                    Console.WriteLine($"⚠️ Không tìm thấy dataset cho label {review.LabelId} → tạo mới");

                    dataset = new Dataset
                    {
                        DatasetName = $"Auto_Label_{review.LabelId}",
                        LabelId = review.LabelId,
                        ParentDatasetId = parentDatasetId,
                        ProjectId = 1, // ⚠️ chỉnh lại nếu cần
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Datasets.Add(dataset);
                    await _context.SaveChangesAsync(); // cần để có DatasetId
                }

                // 🔍 check duplicate
                var exists = await _context.DataItems.AnyAsync(d =>
                    d.FileUrl == originalItem.FileUrl &&
                    d.DatasetId == dataset.DatasetId
                );

                if (exists)
                {
                    Console.WriteLine($"⚠️ Exists item {itemId}");
                    continue;
                }

                Console.WriteLine($"✅ PUSH item {itemId} → {dataset.DatasetName}");

                var originalItemIdToUse = originalItem.OriginalItemId ?? originalItem.ItemId;

                _context.DataItems.Add(new DataItem
                {
                    DatasetId = dataset.DatasetId,
                    OriginalItemId = originalItemIdToUse,
                    FileUrl = originalItem.FileUrl,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 🔥 SAVE CUỐI
            await _context.SaveChangesAsync();
        }
    }
}
