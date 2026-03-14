using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class CreateTaskRequest
    {
        public int DatasetRoundId { get; set; }
        public int AssigneeUserId { get; set; }
        //public TaskType Type { get; set; } = TaskType.Labeling;
        public int GroupNumber { get; set; }
        public int? ParentTaskId { get; set; }
    }
}