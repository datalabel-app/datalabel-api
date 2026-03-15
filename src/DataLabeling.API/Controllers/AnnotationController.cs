using DataLabeling.API.DTOs;
using DataLabeling.DAL.Data;
using DataLabeling.DTOs.Annotations;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/annotations")]
[ApiController]
[Authorize]
public class AnnotationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnnotationController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpPost("classification")]
    public async Task<IActionResult> CreateClassification(CreateAnnotationDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var task = await _context.Tasks
            .FirstOrDefaultAsync(x => x.TaskId == dto.TaskId);

        if (task == null)
            return BadRequest("Task not found");

        var annotation = new Annotation
        {
            TaskId = task.TaskId,
            ItemId = task.DataItemId,
            LabelId = dto.LabelId,
            RoundId = dto.RoundId,
            AnnotatorId = userId,
            ShapeType = "classification",
            Coordinates = "",
            Classification = dto.Classification,
            CreatedAt = DateTime.UtcNow
        };

        _context.Annotations.Add(annotation);

        task.Status = DataLabeling.Entities.TaskStatus.Annotating;
        task.AnnotatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Classification created",
            annotationId = annotation.AnnotationId
        });
    }

}