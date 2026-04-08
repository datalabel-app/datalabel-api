using DataLabeling.BLL; // EmailService
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class TaskDeadlineChecker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskDeadlineChecker> _logger;

    public TaskDeadlineChecker(IServiceProvider serviceProvider, ILogger<TaskDeadlineChecker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskDeadlineChecker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyOverdueTasksAsync();
                await CheckAndNotifyUpcomingTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking task deadlines.");
            }
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async System.Threading.Tasks.Task CheckAndNotifyOverdueTasksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var now = DateTime.UtcNow.Date;

        var overdueTasks = await dbContext.Tasks
            .Include(t => t.Round)
                .ThenInclude(r => r.Dataset)
                    .ThenInclude(d => d.Project)
                        .ThenInclude(p => p.Manager)
            .Where(t => t.Deadline.HasValue
                        && t.Deadline < DateTime.UtcNow
                        && t.Status != DataLabeling.Entities.TaskStatus.Done
                        && (t.LastNotifiedAt == null || t.LastNotifiedAt.Value.Date < now))
            .ToListAsync();

        if (!overdueTasks.Any())
        {
            _logger.LogInformation("No overdue tasks found at {time}", DateTime.UtcNow);
            return;
        }

        foreach (var task in overdueTasks)
        {
            var manager = task.Round.Dataset.Project.Manager;
            if (manager == null || string.IsNullOrEmpty(manager.Email))
            {
                _logger.LogWarning("Task #{taskId} has no manager assigned.", task.TaskId);
                continue;
            }

            var managerEmail = manager.Email;
            var managerName = manager.FullName;

            _logger.LogInformation("Sending overdue notification for TaskId {taskId} to {email}", task.TaskId, managerEmail);

            var subject = $"Task #{task.TaskId} đã quá hạn - Data Labeling";
            var htmlContent = $@"
            <h2>Xin chào {managerName},</h2>
            <p>Task <b>#{task.TaskId}</b> đã quá hạn.</p>
            <p>Mô tả: {task.DescriptionError ?? ""}</p>
            <p>Vui lòng kiểm tra hệ thống để cập nhật tiến độ.</p>
            <br/>
            <p>Trân trọng,<br/>Data Labeling Team</p>
        ";
            var plainTextContent = $"Xin chào {managerName},\n\nTask #{task.TaskId} đã quá hạn.\nMô tả: {task.DescriptionError ?? ""}\n\nVui lòng kiểm tra hệ thống để cập nhật tiến độ.\n\nTrân trọng,\nData Labeling Team";

            await emailService.SendEmailAsync(managerEmail, managerName, subject, htmlContent, plainTextContent);

            // 🔹 đánh dấu đã gửi email hôm nay
            task.IsOverdueNotified = true;
            task.LastNotifiedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
    private async System.Threading.Tasks.Task CheckAndNotifyUpcomingTasksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var now = DateTime.UtcNow;
        var tomorrow = now.AddDays(1).Date;

        var upcomingTasks = await dbContext.Tasks
            .Include(t => t.Round)
                .ThenInclude(r => r.Dataset)
                    .ThenInclude(d => d.Project)
            .Where(t => t.Deadline.HasValue
                        && t.Status != DataLabeling.Entities.TaskStatus.Done
                        && t.AnnotatorId != null
                        // chỉ gửi 1 ngày trước deadline
                        && t.Deadline.Value.Date == tomorrow
                        // tránh gửi nhắc nhiều lần trong ngày
                        && (t.LastNotifiedAt == null || t.LastNotifiedAt.Value.Date < now.Date))
            .ToListAsync();

        if (!upcomingTasks.Any())
            return;

        foreach (var task in upcomingTasks)
        {
            var annotator = task.Annotator;
            if (annotator == null || string.IsNullOrEmpty(annotator.Email))
            {
                _logger.LogWarning("Task #{taskId} has no annotator assigned.", task.TaskId);
                continue;
            }

            var subject = $"Task #{task.TaskId} sắp hết hạn - Data Labeling";
            var htmlContent = $@"
            <h2>Xin chào {annotator.FullName},</h2>
            <p>Task <b>#{task.TaskId}</b> của bạn sắp hết hạn vào {task.Deadline:dd/MM/yyyy}.</p>
            <p>Mô tả: {task.DescriptionError ?? ""}</p>
            <p>Vui lòng hoàn thành tiến độ trước deadline.</p>
            <br/>
            <p>Trân trọng,<br/>Data Labeling Team</p>
        ";
            var plainTextContent = $"Xin chào {annotator.FullName},\n\nTask #{task.TaskId} của bạn sắp hết hạn vào {task.Deadline:dd/MM/yyyy}.\nMô tả: {task.DescriptionError ?? ""}\n\nVui lòng hoàn thành tiến độ trước deadline.\n\nTrân trọng,\nData Labeling Team";

            await emailService.SendEmailAsync(annotator.Email, annotator.FullName, subject, htmlContent, plainTextContent);

            task.LastNotifiedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
}