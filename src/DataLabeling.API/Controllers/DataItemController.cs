using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/dataitems")]
    [ApiController]
    public class DataItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dataset/{datasetId}/unassigned")]
        public async Task<IActionResult> GetUnassignedByDataset(int datasetId)
        {
            var items = await _context.DataItems
                .Where(d => d.DatasetId == datasetId
                    && !d.TaskDataItems.Any())
                .OrderBy(d => d.ItemId)
                .Select(d => new DataItemResponse
                {
                    ItemId = d.ItemId,
                    DatasetId = d.DatasetId,
                    FileUrl = d.FileUrl,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("dataset/{datasetId}")]
        public async Task<IActionResult> GetByDataset(int datasetId, int? labelId = null)
        {
            // 1️⃣ Lấy tất cả dataset cha + con
            var allDatasetIds = await GetAllDatasetIdsRecursive(datasetId);

            // 2️⃣ Lấy tất cả DataItem trong các dataset này
            var dataItems = await _context.DataItems
                .Where(di => allDatasetIds.Contains(di.DatasetId))
                .OrderBy(di => di.ItemId)
                .ToListAsync();

            if (!dataItems.Any())
                return Ok(new List<object>());

            var itemIds = dataItems.Select(di => di.ItemId).ToList();

            var approvedItemIds = await _context.TaskDataItems
                                .Where(tdi => itemIds.Contains(tdi.DataItemId) && tdi.ReviewStatus == "Approved")
                                .Select(tdi => tdi.DataItemId)
                                .Distinct()
                                .ToListAsync();

            // 3️⃣ Lấy tất cả Annotation kèm Label
            var annotations = await _context.Annotations
                .Where(a => approvedItemIds.Contains(a.ItemId))
                .Include(a => a.Label)
                .ToListAsync();

            // 4️⃣ Group DataItem theo OriginalItemId (nếu null thì dùng ItemId)
            var grouped = dataItems
                .GroupBy(di => di.OriginalItemId ?? di.ItemId)
                .Select(g =>
                {
                    // Tập hợp tất cả label của cha + con
                    var labelsForGroup = annotations
                        .Where(a => g.Select(di => di.ItemId).Contains(a.ItemId) && a.Label != null)
                        .Select(a => a.Label.LabelName)
                        .Distinct()
                        .ToList();

                    return new DataItemResponse
                    {
                        ItemId = g.Key, // Đây là ItemId cha
                        DatasetId = g.First().DatasetId,
                        FileUrl = g.First().FileUrl, // lấy file của cha
                        Status = g.First().Status,
                        CreatedAt = g.First().CreatedAt,
                        LabelCount = labelsForGroup.Count,
                        Labels = labelsForGroup
                    };
                })
                .ToList();

            // 5️⃣ Filter theo labelId nếu cần
            if (labelId.HasValue)
            {
                grouped = grouped
                    .Where(di => annotations.Any(a => (dataItems.FirstOrDefault(d => d.ItemId == di.ItemId)?.ItemId ?? 0) == a.ItemId
                                                       && a.LabelId == labelId.Value))
                    .ToList();
            }

            return Ok(grouped);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataItemResponse>>> GetAll()
        {
            var items = await _context.DataItems
                .Select(x => new DataItemResponse
                {
                    ItemId = x.ItemId,
                    DatasetId = x.DatasetId,
                    FileUrl = x.FileUrl,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataItemResponse>> GetById(int id)
        {
            var item = await _context.DataItems
                .Where(x => x.ItemId == id)
                .Select(x => new DataItemResponse
                {
                    ItemId = x.ItemId,
                    DatasetId = x.DatasetId,
                    FileUrl = x.FileUrl,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(item);
        }


        [HttpPost]
        public async Task<ActionResult<DataItemResponse>> Create(CreateDataItemRequest dto)
        {
            var item = new DataItem
            {
                DatasetId = dto.DatasetId,
                FileUrl = dto.FileUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataItems.Add(item);
            await _context.SaveChangesAsync();

            var result = new DataItemResponse
            {
                ItemId = item.ItemId,
                DatasetId = item.DatasetId,
                FileUrl = item.FileUrl,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = item.ItemId }, result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DataItems.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.DataItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
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
                var subChildren = await GetAllSubDatasets(child.DatasetId);
                result.AddRange(subChildren);
            }

            return result;
        }

        private async Task<List<int>> GetAllDatasetIdsRecursive(int parentId)
        {
            var allIds = new List<int> { parentId };
            var childIds = await _context.Datasets
                .Where(d => d.ParentDatasetId == parentId)
                .Select(d => d.DatasetId)
                .ToListAsync();

            foreach (var childId in childIds)
            {
                var subIds = await GetAllDatasetIdsRecursive(childId);
                allIds.AddRange(subIds);
            }

            return allIds;
        }
    }
}
