namespace DataLabeling.API.DTOs
{
    public class TaskDetailDto
    {
        public int TaskId { get; set; }

        public int RoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public int ShapeType { get; set; }

        public string? AnnotatorName { get; set; }

        public string? ReviewerName { get; set; }

        public List<TaskDataItemDto> DataItems { get; set; } = new();
    }
}
