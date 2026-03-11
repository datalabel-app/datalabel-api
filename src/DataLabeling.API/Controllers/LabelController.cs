using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LabelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LabelController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelRequest request)
        {
            var round = await _context.DatasetRounds.FindAsync(request.RoundId);

            if (round == null)
                return BadRequest("Round not found");

            var label = new Label
            {
                RoundId = request.RoundId,
                LabelName = request.LabelName,
                Description = request.Description
            };

            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var response = new LabelResponse
            {
                LabelId = label.LabelId,
                RoundId = label.RoundId,
                LabelName = label.LabelName,
                Description = label.Description
            };

            return Ok(response);
        }
    }
}