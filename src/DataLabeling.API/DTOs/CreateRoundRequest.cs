using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class CreateRoundRequest
    {
        public int DatasetId { get; set; }

        public int RoundNumber { get; set; }

        public string? Description { get; set; }

        public ShapeType ShapeType { get; set; } = ShapeType.Bbox;
    }
}
