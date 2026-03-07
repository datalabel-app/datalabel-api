using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataItemController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateDataItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d =>
                    d.DatasetId == request.DatasetId &&
                    d.Project.ManagerId == userId);

            if (dataset == null)
                return BadRequest("Dataset not found or not yours");

            var entity = new DataItem
            {
                DatasetId = request.DatasetId,
                FileUrl = request.FileUrl,
                Status = request.Status
            };

            _context.DataItems.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(entity));
        }

        private static DataItemResponse MapToResponse(DataItem entity)
        {
            return new DataItemResponse
            {
                ItemId = entity.ItemId,
                DatasetId = entity.DatasetId,
                FileUrl = entity.FileUrl,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
