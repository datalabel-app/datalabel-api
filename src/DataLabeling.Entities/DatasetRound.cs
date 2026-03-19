using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLabeling.Entities
{
    public class DatasetRound
    {
        [Key]
        public int RoundId { get; set; }

        public int DatasetId { get; set; }

        public int RoundNumber { get; set; }

        public string? Description { get; set; }

        public ShapeType ShapeType { get; set; } = ShapeType.Bbox;

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dataset Dataset { get; set; } = null!;

        public ICollection<Label> Labels { get; set; } = new List<Label>();

        public ICollection<DataLabeling.Entities.Task> Tasks { get; set; } = new List<DataLabeling.Entities.Task>();
    }
}