using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.DTOs;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/labels")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LabelController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> RequestLabel([FromBody] RequestLabelDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LabelName))
            {
                return BadRequest(new { message = "Label name is required" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token" });

            int annotatorId = int.Parse(userIdClaim);

            var labelName = dto.LabelName.Trim();

            var existing = await _context.Labels
                .FirstOrDefaultAsync(x =>
                    x.RoundId == dto.RoundId &&
                    x.LabelName.ToLower() == labelName.ToLower()
                );

            if (existing != null)
            {
                return BadRequest(new { message = "Label already exists" });
            }

            var label = new Label
            {
                RoundId = dto.RoundId,
                LabelName = labelName,
                Description = dto.Description,
                LabelStatus = LabelStatus.Pending,
                AnnotatorId = annotatorId
            };

            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request label successfully",
                data = label
            });
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
                Description = request.Description,
                LabelStatus = LabelStatus.Approved,
            };

            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var response = new LabelResponse
            {
                LabelId = label.LabelId,
                RoundId = label.RoundId,
                LabelName = label.LabelName,
                Description = label.Description,

            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLabels()
        {
            var labels = await _context.Labels
                .Include(l => l.Round)
                .OrderByDescending(l => l.LabelId)
                .ToListAsync();

            return Ok(labels);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetLabel(int id)
        {
            var label = await _context.Labels
                .Include(l => l.Round)
                .FirstOrDefaultAsync(l => l.LabelId == id);

            if (label == null)
                return NotFound("Label not found");

            return Ok(label);
        }

        [Authorize]
        [HttpGet("my-labels")]
        public async Task<IActionResult> GetMyLabels()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);

            var labels = await _context.Labels
                .Where(l => l.AnnotatorId == userId)
                .Select(l => new
                {
                    l.LabelId,
                    l.LabelName,
                    l.Description,
                    l.LabelStatus,

                    Round = new
                    {
                        l.Round.RoundId,
                        l.Round.RoundNumber,
                        l.Round.ShapeType,
                        l.Round.Description
                    },

                    Dataset = new
                    {
                        l.Round.Dataset.DatasetId,
                        l.Round.Dataset.DatasetName
                    },

                    Project = new
                    {
                        l.Round.Dataset.Project.ProjectId,
                        l.Round.Dataset.Project.ProjectName
                    }
                })
                .OrderByDescending(l => l.LabelId)
                .ToListAsync();

            return Ok(labels);
        }

        [HttpGet("pending/project/{projectId}")]
        public async Task<IActionResult> GetPendingLabelsByProject(int projectId)
        {
            var labels = await _context.Labels
                .Where(l => l.LabelStatus == LabelStatus.Pending &&
                            l.Round.Dataset.ProjectId == projectId)
                .Select(l => new
                {
                    l.LabelId,
                    l.LabelName,
                    l.Description,
                    l.LabelStatus,

                    Round = new
                    {
                        l.Round.RoundId,
                        l.Round.RoundNumber,
                        l.Round.ShapeType,
                        l.Round.Description
                    },

                    Dataset = new
                    {
                        l.Round.Dataset.DatasetId,
                        l.Round.Dataset.DatasetName
                    },

                    Project = new
                    {
                        l.Round.Dataset.Project.ProjectId,
                        l.Round.Dataset.Project.ProjectName
                    },

                    Annotator = l.Annotator == null ? null : new
                    {
                        l.Annotator.UserId,
                        l.Annotator.FullName,
                        l.Annotator.Email
                    }
                })
                .OrderByDescending(l => l.LabelId)
                .ToListAsync();

            return Ok(labels);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveLabel(int id)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label == null)
            {
                return NotFound(new { message = "Label not found" });
            }

            if (label.LabelStatus == LabelStatus.Approved)
            {
                return BadRequest(new { message = "Label already approved" });
            }

            label.LabelStatus = LabelStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Label approved successfully",
                data = label
            });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectLabel(int id)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label == null)
            {
                return NotFound(new { message = "Label not found" });
            }

            if (label.LabelStatus == LabelStatus.Rejected)
            {
                return BadRequest(new { message = "Label already rejected" });
            }

            label.LabelStatus = LabelStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Label rejected",
                data = label
            });
        }

        [HttpGet("round/{roundId}")]
        public async Task<IActionResult> GetLabelsByRound(int roundId)
        {
            var labels = await _context.Labels
                .Where(l => l.RoundId == roundId
                         && l.LabelStatus == LabelStatus.Approved)
                .OrderBy(l => l.LabelName)
                .ToListAsync();

            return Ok(labels);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLabel(int id, [FromBody] Label request)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label == null)
                return NotFound("Label not found");

            label.LabelName = request.LabelName;
            label.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(label);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabel(int id)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label == null)
                return NotFound("Label not found");

            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();

            return Ok("Label deleted");
        }
    }
}