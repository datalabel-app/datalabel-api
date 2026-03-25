using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace DataLabeling.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var totalDatasets = await _context.Datasets.CountAsync();
            var totalTasks = await _context.Tasks.CountAsync();

            var pendingTasks = await _context.Tasks
                .CountAsync(t => t.Status == DataLabeling.Entities.TaskStatus.Pending);

            var completedTasks = await _context.Tasks
                .CountAsync(t => t.Status == DataLabeling.Entities.TaskStatus.Done);

            var totalLabels = await _context.Labels.CountAsync();

            var pendingLabels = await _context.Labels
                .CountAsync(l => l.LabelStatus == LabelStatus.Pending);

            var approvedLabels = await _context.Labels
                .CountAsync(l => l.LabelStatus == LabelStatus.Approved);

            var rejectedLabels = await _context.Labels
                .CountAsync(l => l.LabelStatus == LabelStatus.Rejected);

            return Ok(new
            {
                totalUsers,
                totalProjects,
                totalDatasets,
                totalTasks,
                tasks = new
                {
                    pending = pendingTasks,
                    completed = completedTasks
                },
                labels = new
                {
                    total = totalLabels,
                    pending = pendingLabels,
                    approved = approvedLabels,
                    rejected = rejectedLabels
                }
            });
        }


        [Authorize]
        [HttpGet("manager")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token" });

            int managerId = int.Parse(userIdClaim);

            var projectIds = await _context.Projects
                .Where(p => p.ManagerId == managerId)
                .Select(p => p.ProjectId)
                .ToListAsync();

            var datasetIds = await _context.Datasets
                .Where(d => projectIds.Contains(d.ProjectId))
                .Select(d => d.DatasetId)
                .ToListAsync();

            var roundIds = await _context.DatasetRounds
                .Where(r => datasetIds.Contains(r.DatasetId))
                .Select(r => r.RoundId)
                .ToListAsync();

            var totalTasks = await _context.Tasks
                .CountAsync(t => roundIds.Contains(t.RoundId));

            var pendingTasks = await _context.Tasks
                .CountAsync(t => roundIds.Contains(t.RoundId)
                                 && t.Status == DataLabeling.Entities.TaskStatus.Pending);

            var completedTasks = await _context.Tasks
                .CountAsync(t => roundIds.Contains(t.RoundId)
                                 && t.Status == DataLabeling.Entities.TaskStatus.Done);

            var pendingLabels = await _context.Labels
                .CountAsync(l => roundIds.Contains(l.RoundId)
                                 && l.LabelStatus == LabelStatus.Pending);

            var approvedLabels = await _context.Labels
                .CountAsync(l => roundIds.Contains(l.RoundId)
                                 && l.LabelStatus == LabelStatus.Approved);

            var rejectedLabels = await _context.Labels
                .CountAsync(l => roundIds.Contains(l.RoundId)
                                 && l.LabelStatus == LabelStatus.Rejected);

            return Ok(new
            {
                totalProjects = projectIds.Count,
                totalDatasets = datasetIds.Count,
                totalTasks,
                tasks = new
                {
                    pending = pendingTasks,
                    completed = completedTasks
                },
                labels = new
                {
                    pending = pendingLabels,
                    approved = approvedLabels,
                    rejected = rejectedLabels
                }
            });
        }
    }
}