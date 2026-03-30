using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class DatasetRoundResponse
    {
        public int RoundId { get; set; }

        public int DatasetId { get; set; }

        public int RoundNumber { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public ShapeType ShapeType { get; set; } = ShapeType.Bbox;

        public List<LabelResponse>? Labels { get; set; }
    }
}