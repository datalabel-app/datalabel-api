namespace DataLabeling.API.DTOs
{
    public class TaskResponseDto
    {
        public int TaskId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public string? AnnotatorName { get; set; }

        public DataLabeling.Entities.TaskStatus? Status { get; set; }

        public string? ReviewerName { get; set; }
        public string? DatasetName { get; set; }
        public int DataItemCount { get; set; }

        public int ShapeType { get; set; }
    }
}
