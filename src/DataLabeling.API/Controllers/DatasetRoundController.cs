<<<<<<< Updated upstream
﻿using DataLabeling.API.DTOs;
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
    }
=======
﻿using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/datasetrounds")]
    [Authorize]
    public class DatasetRoundController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatasetRoundController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateRound([FromBody] CreateRoundRequest request)
        {
            var dataset = await _context.Datasets.FindAsync(request.DatasetId);

            if (dataset == null)
                return BadRequest("Dataset not found");

            var round = new DatasetRound
            {
                DatasetId = request.DatasetId,
                RoundNumber = request.RoundNumber,
                Description = request.Description,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.DatasetRounds.Add(round);
            await _context.SaveChangesAsync();

            var response = new DatasetRoundResponse
            {
                RoundId = round.RoundId,
                DatasetId = round.DatasetId,
                RoundNumber = round.RoundNumber,
                Description = round.Description,
                Status = round.Status,
                CreatedAt = round.CreatedAt
            };

            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllRounds()
        {
            var rounds = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(rounds);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRound(int id)
        {
            var round = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .Include(r => r.Labels)
                .FirstOrDefaultAsync(r => r.RoundId == id);

            if (round == null)
                return NotFound("Round not found");

            return Ok(round);
        }


        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetRoundsByDataset(int datasetId)
        {
            var rounds = await _context.DatasetRounds
                .Where(r => r.DatasetId == datasetId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();

            return Ok(rounds);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRound(int id, [FromBody] DatasetRound request)
        {
            var round = await _context.DatasetRounds.FindAsync(id);

            if (round == null)
                return NotFound("Round not found");

            round.RoundNumber = request.RoundNumber;
            round.Description = request.Description;
            round.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(round);
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var round = await _context.DatasetRounds.FindAsync(id);

            if (round == null)
                return NotFound("Round not found");

            round.Status = status;

            await _context.SaveChangesAsync();

            return Ok(round);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRound(int id)
        {
            var round = await _context.DatasetRounds
                .Include(r => r.Labels)
                .FirstOrDefaultAsync(r => r.RoundId == id);

            if (round == null)
                return NotFound("Round not found");

            _context.DatasetRounds.Remove(round);
            await _context.SaveChangesAsync();

            return Ok("Round deleted");
        }
    }
>>>>>>> Stashed changes
}