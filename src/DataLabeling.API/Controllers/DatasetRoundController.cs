using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using DataLabeling.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DatasetRoundController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatasetRoundController(ApplicationDbContext context)
        {
            _context = context;
        }

   
        [HttpPost]
        public async Task<IActionResult> Create(CreateDatasetRoundRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d =>
                    d.DatasetId == request.DatasetId &&
                    d.Project.ManagerId == userId);

            if (dataset == null)
                return BadRequest("Dataset not found or not yours");

            var entity = new DatasetRound
            {
                DatasetId = request.DatasetId,
                RoundId = request.RoundId,
                Status = request.Status,
            };

            if (entity.Status == DatasetRoundStatus.Completed)
                entity.CompletedAt = DateTime.UtcNow;

            _context.DatasetRounds.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(entity));
        }

      
        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetByDataset(int datasetId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d =>
                    d.DatasetId == datasetId &&
                    d.Project.ManagerId == userId);

            if (dataset == null)
                return BadRequest("Dataset not found or not yours");

            var rounds = await _context.DatasetRounds
                .Where(r => r.DatasetId == datasetId)
                .Select(r => MapToResponse(r))
                .ToListAsync();

            return Ok(rounds);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDatasetRoundRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(r =>
                    r.DatasetRoundId == id &&
                    r.Dataset.Project.ManagerId == userId);

            if (round == null)
                return NotFound("DatasetRound not found");

            if (request.Status.HasValue)
            {
                round.Status = request.Status.Value;

                if (round.Status == DatasetRoundStatus.Completed)
                    round.CompletedAt = DateTime.UtcNow;
                else
                    round.CompletedAt = null;
            }

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(round));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .ThenInclude(d => d.Project)
                .FirstOrDefaultAsync(r =>
                    r.DatasetRoundId == id &&
                    r.Dataset.Project.ManagerId == userId);

            if (round == null)
                return NotFound("DatasetRound not found");

            _context.DatasetRounds.Remove(round);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

    
        private static DatasetRoundResponse MapToResponse(DatasetRound entity)
        {
            return new DatasetRoundResponse
            {
                DatasetRoundId = entity.DatasetRoundId,
                DatasetId = entity.DatasetId,
                RoundId = entity.RoundId,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt
            };
        }
    }
}