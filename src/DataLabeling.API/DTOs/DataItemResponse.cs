using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class DataItemResponse
    {
        public int ItemId { get; set; }

        public int DatasetId { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public int? AnnotatorId { get; set; }

        public string? AnnotatorName { get; set; }

        public int? ReviewerId { get; set; }

        public string? ReviewerName { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int? LabelCount { get; set; }

        public List<string>? Labels { get; set; } = new List<string>();
    }
}