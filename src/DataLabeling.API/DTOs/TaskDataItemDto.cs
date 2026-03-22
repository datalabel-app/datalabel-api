using DataLabeling.DTOs.Annotations;

namespace DataLabeling.API.DTOs
{
    public class TaskDataItemDto
    {
        public int ItemId { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ReviewStatus { get; set; } = "Pending";
        public string? ReviewComment { get; set; }

        public string? ErrorMessage { get; set; }

        public int? AnnotationId { get; set; }
        public List<AnnotationResponse>? Annotations { get; set; }
        public int? LabelId { get; set; }
    }
}
