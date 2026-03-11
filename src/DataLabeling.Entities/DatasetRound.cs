using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataLabeling.Entities.Enums;

namespace DataLabeling.Entities
{
    public class DatasetRound
    {
        [Key]
        public int DatasetRoundId { get; set; }

        [ForeignKey("Dataset")]
        public int DatasetId { get; set; }

        public int RoundId { get; set; }

        public DatasetRoundStatus Status { get; set; } = DatasetRoundStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public Dataset Dataset { get; set; } = null!;

        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}