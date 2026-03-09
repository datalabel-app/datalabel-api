using DataLabeling.Entities;
namespace DataLabeling.API.DTOs
{
    public class TaskResponse
    {
        public int TaskId { get; set; }
        public int DatasetRoundId { get; set; }
        public int AssigneeUserId { get; set; }
        public TaskType Type { get; set; }
        public Entities.TaskStatus Status { get; set; }
        public int GroupNumber { get; set; }
        public int? ParentTaskId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}