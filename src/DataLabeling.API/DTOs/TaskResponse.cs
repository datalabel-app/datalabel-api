namespace DataLabeling.API.DTOs
{
    public class TaskResponse
    {
        public int TaskId { get; set; }

        public int DataItemId { get; set; }

        public int RoundId { get; set; }

        public int? AnnotatorId { get; set; }

        public string? AnnotatorName { get; set; }

        public int? ReviewerId { get; set; }

        public string? ReviewerName { get; set; }

        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? AnnotatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string FileUrl { get; set; } = "";
    }
}
