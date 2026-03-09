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
