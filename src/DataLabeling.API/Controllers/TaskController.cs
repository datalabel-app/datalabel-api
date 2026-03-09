using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var datasetRound = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(r =>
                    r.DatasetRoundId == request.DatasetRoundId &&
                    r.Dataset.Project.ManagerId == userId);

            if (datasetRound == null)
                return BadRequest("DatasetRound not found or not yours");

            var entity = new DataLabeling.Entities.Task
            {
                DatasetRoundId = request.DatasetRoundId,
                AssigneeUserId = request.AssigneeUserId,
                Type = request.Type,
                GroupNumber = request.GroupNumber,
                ParentTaskId = request.ParentTaskId
            };

            _context.Set<DataLabeling.Entities.Task>().Add(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(entity));
        }


        [HttpGet("round/{roundId}")]
        public async Task<IActionResult> GetByRound(int roundId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var datasetRound = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(r =>
                    r.DatasetRoundId == roundId &&
                    r.Dataset.Project.ManagerId == userId);

            if (datasetRound == null)
                return BadRequest("DatasetRound not found or not yours");

            var tasks = await _context.Set<Entities.Task>()
                .Where(t => t.DatasetRoundId == roundId)
                .Select(t => MapToResponse(t))
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var task = await _context.Set<DataLabeling.Entities.Task>()
                .Include(t => t.DatasetRound)
                .ThenInclude(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(t =>
                    t.TaskId == id &&
                    t.DatasetRound.Dataset.Project.ManagerId == userId);

            if (task == null)
                return NotFound("Task not found");

            if (request.Status.HasValue)
            {
                task.Status = request.Status.Value;

                if (task.Status == DataLabeling.Entities.TaskStatus.Completed)
                    task.CompletedAt = DateTime.UtcNow;
                else
                    task.CompletedAt = null;
            }

            if (request.Type.HasValue)
                task.Type = request.Type.Value;

            if (request.GroupNumber.HasValue)
                task.GroupNumber = request.GroupNumber.Value;

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(task));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var task = await _context.Set<DataLabeling.Entities.Task>()
                .Include(t => t.DatasetRound)
                .ThenInclude(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(t =>
                    t.TaskId == id &&
                    t.DatasetRound.Dataset.Project.ManagerId == userId);

            if (task == null)
                return NotFound("Task not found");

            _context.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        private static TaskResponse MapToResponse(Entities.Task entity)
        {
            return new TaskResponse
            {
                TaskId = entity.TaskId,
                DatasetRoundId = entity.DatasetRoundId,
                AssigneeUserId = entity.AssigneeUserId,
                Type = entity.Type,
                Status = entity.Status,
                GroupNumber = entity.GroupNumber,
                ParentTaskId = entity.ParentTaskId,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt
            };
        }
    }
}
