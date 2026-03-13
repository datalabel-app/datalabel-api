using DataLabeling.API.DTOs;
using DataLabeling.DAL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataLabeling.API.Controllers
{
    [Route("api/datasetrounds")]
    [ApiController]
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
                ShapeType = request.ShapeType,
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
                ShapeType = round.ShapeType,
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

    }
}