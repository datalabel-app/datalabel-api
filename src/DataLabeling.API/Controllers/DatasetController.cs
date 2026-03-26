using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataLabeling.API.Controllers
{
    [Route("api/datasets")]
    [ApiController]
    public class DatasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatasetController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateDataset([FromBody] CreateDatasetRequest request)
        {
            var project = await _context.Projects.FindAsync(request.ProjectId);

            if (project == null)
                return BadRequest("Project not found");

            var dataset = new Dataset
            {
                ProjectId = request.ProjectId,
                DatasetName = request.DatasetName,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Datasets.Add(dataset);
            await _context.SaveChangesAsync();

            var response = new DatasetResponse
            {
                DatasetId = dataset.DatasetId,
                ProjectId = dataset.ProjectId,
                DatasetName = dataset.DatasetName,
                Status = dataset.Status
            };

            return Ok(response);
        }
        [HttpGet("{datasetId}/labels")]
        public async Task<IActionResult> GetLabelsFromRoot(int datasetId)
        {
            var root = await GetRootDataset(datasetId);

            var allChildIds = await GetAllChildDatasetIds(root.DatasetId);

            allChildIds.Add(root.DatasetId);

            var labels = await _context.Datasets
                .Where(d => allChildIds.Contains(d.DatasetId) && d.LabelId != null)
                .Select(d => new
                {
                    d.DatasetId,
                    d.LabelId,
                    d.Label!.LabelName
                })
                .Distinct()
                .ToListAsync();

            return Ok(labels);
        }

        [HttpGet("tree/{projectId}")]
        public async Task<IActionResult> GetDatasetTree(int projectId)
        {
            var datasets = await _context.Datasets
                   .Include(d => d.Rounds)
                .Where(d => d.ProjectId == projectId)

                .ToListAsync();

            var tree = BuildTree(datasets, null);

            return Ok(tree);
        }

        [HttpGet("export/{datasetId}")]
        public async Task<IActionResult> ExportDataset(int datasetId)
        {
            var dataset = await _context.Datasets.FindAsync(datasetId);
            if (dataset == null)
                return NotFound("Dataset not found");

            var allDatasetIds = new List<int> { datasetId };
            var subDatasets = await GetAllSubDatasets(datasetId);
            allDatasetIds.AddRange(subDatasets.Select(d => d.DatasetId));

            var dataItems = await _context.DataItems
                .Where(di => allDatasetIds.Contains(di.DatasetId))
                .ToListAsync();

            if (!dataItems.Any())
                return Ok(new List<object>());

            var itemIds = dataItems.Select(di => di.ItemId).ToList();

            var annotations = await _context.Annotations
                .Where(a => itemIds.Contains(a.ItemId))
                .Include(a => a.Label)
                .ToListAsync();

            var groupedByOriginalItem = dataItems
                .Where(di => di.Status == "Annotated")
                .GroupBy(di => di.OriginalItemId ?? di.ItemId)
                .Select(g => new
                {
                    FileUrl = g.First().FileUrl,
                    Annotations = annotations
                        .Where(a => g.Select(di => di.ItemId).Contains(a.ItemId))
                        .Select(a => new
                        {
                            Label = a.Label.LabelName,
                            ShapeType = a.ShapeType,
                            Coordinates = a.Coordinates,
                        })
                        .Distinct()
                        .ToList()
                })
                .ToList();

            return Ok(groupedByOriginalItem);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDatasets()
        {
            var datasets = await _context.Datasets
                .Include(d => d.Project)
                .Include(d => d.Rounds)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(datasets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataset(int id)
        {
            var dataset = await _context.Datasets
                .Include(d => d.Project)
                .Include(d => d.DataItems)
                .Include(d => d.Rounds)
                .FirstOrDefaultAsync(d => d.DatasetId == id);

            if (dataset == null)
                return NotFound("Dataset not found");

            var result = new DatasetDetailDto
            {
                DatasetId = dataset.DatasetId,
                DatasetName = dataset.DatasetName,
                ParentDatasetId = dataset.ParentDatasetId,
                Status = dataset.Status,

                Project = new ProjectDto
                {
                    ProjectId = dataset.Project.ProjectId,
                    ProjectName = dataset.Project.ProjectName
                },




            };

            return Ok(result);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetDatasetsByProject(int projectId)
        {
            var datasets = await _context.Datasets
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.Rounds)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(datasets);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataset(int id, UpdateDatasetRequest request)
        {
            var dataset = await _context.Datasets.FindAsync(id);

            if (dataset == null)
                return NotFound("Dataset not found");

            dataset.DatasetName = request.DatasetName;
            dataset.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(dataset);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataset(int id)
        {
            var dataset = await _context.Datasets
                .Include(d => d.DataItems)
                .Include(d => d.Rounds)
                .FirstOrDefaultAsync(d => d.DatasetId == id);

            if (dataset == null)
                return NotFound("Dataset not found");

            var childDatasets = await _context.Datasets
                .Where(d => d.ParentDatasetId == id)
                .ToListAsync();

            _context.Datasets.RemoveRange(childDatasets);

            _context.DataItems.RemoveRange(dataset.DataItems);
            _context.DatasetRounds.RemoveRange(dataset.Rounds);

            _context.Datasets.Remove(dataset);

            await _context.SaveChangesAsync();

            return Ok("Dataset deleted");
        }

        private List<DatasetTreeResponse> BuildTree(List<Dataset> all, int? parentId)
        {
            return all

                .Where(d => d.ParentDatasetId == parentId)

                .Select(d => new DatasetTreeResponse
                {
                    DatasetId = d.DatasetId,
                    DatasetName = d.DatasetName,
                    Status = d.Status,

                    ShapeType = d.Rounds

                    .Select(r => (int?)r.ShapeType)
                    .FirstOrDefault(),

                    Children = BuildTree(all, d.DatasetId)
                })
                .ToList();
        }

        private async Task<Dataset> GetRootDataset(int datasetId)
        {
            var dataset = await _context.Datasets
                .FirstAsync(d => d.DatasetId == datasetId);

            while (dataset.ParentDatasetId != null)
            {
                dataset = await _context.Datasets
                    .FirstAsync(d => d.DatasetId == dataset.ParentDatasetId);
            }

            return dataset;
        }

        private async Task<List<int>> GetAllChildDatasetIds(int rootId)
        {
            var result = new List<int>();
            var queue = new Queue<int>();

            queue.Enqueue(rootId);

            while (queue.Any())
            {
                var currentId = queue.Dequeue();

                var children = await _context.Datasets
                    .Where(d => d.ParentDatasetId == currentId)
                    .Select(d => d.DatasetId)
                    .ToListAsync();

                foreach (var child in children)
                {
                    result.Add(child);
                    queue.Enqueue(child);
                }
            }

            return result;
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
    }
}
