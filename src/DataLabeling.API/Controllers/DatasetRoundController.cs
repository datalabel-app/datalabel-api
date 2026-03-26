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




        [HttpGet]
        public async Task<IActionResult> GetAllRounds()
        {
            var rounds = await _context.DatasetRounds
                .Include(r => r.Dataset)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(rounds);
        }





        [HttpPost]
        public async Task<IActionResult> CreateRound([FromBody] CreateRoundRequest request)
        {
            Console.WriteLine($"👉 Request DatasetId: {request.DatasetId}");

            var dataset = await _context.Datasets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DatasetId == request.DatasetId);

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

            Console.WriteLine($"👉 Before Save: {round.DatasetId}");

            _context.DatasetRounds.Add(round);
            await _context.SaveChangesAsync();

            Console.WriteLine($"👉 After Save: {round.DatasetId}");

            return Ok(new
            {
                round.RoundId,
                round.DatasetId
            });
        }


        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetRoundsWithLeafLabels(int datasetId)
        {
            var datasetIds = await GetSubtreeDatasetIds(datasetId);

            var labels = await _context.Datasets
                .Where(d =>
                    datasetIds.Contains(d.DatasetId) &&
                    d.LabelId != null &&
                    !_context.Datasets.Any(child => child.ParentDatasetId == d.DatasetId)
                )
                .Select(d => new LabelResponse
                {
                    DatasetId = d.DatasetId,
                    LabelId = d.LabelId,
                    LabelName = d.Label!.LabelName
                })
                .ToListAsync();

            var rounds = await _context.DatasetRounds
                .Where(r => r.DatasetId == datasetId)
                .OrderBy(r => r.RoundNumber)
                .Select(r => new DatasetRoundResponse
                {
                    RoundId = r.RoundId,
                    DatasetId = r.DatasetId,
                    RoundNumber = r.RoundNumber,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,

                    Labels = labels
                })
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



        //[HttpGet("dataset/{datasetId}")]
        //public async Task<IActionResult> GetRoundsByDataset(int datasetId)
        //{
        //    var rounds = await _context.DatasetRounds
        //        .Where(r => r.DatasetId == datasetId)
        //        .OrderBy(r => r.RoundNumber)
        //        .ToListAsync();

        //    return Ok(rounds);
        //}




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

        private async Task<List<int>> GetSubtreeDatasetIds(int datasetId)
        {
            var result = new List<int> { datasetId };

            var children = await _context.Datasets
                .Where(d => d.ParentDatasetId == datasetId)
                .ToListAsync();

            foreach (var child in children)
            {
                var sub = await GetSubtreeDatasetIds(child.DatasetId);
                result.AddRange(sub);
            }

            return result;
        }


    }
}