using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLabeling.Entities
{
    public class Label
    {
        [Key]
        public int LabelId { get; set; }

        public int RoundId { get; set; }

        public string LabelName { get; set; } = string.Empty;

        public LabelStatus LabelStatus { get; set; } = LabelStatus.Approved;

        public string? Description { get; set; }

        public int? AnnotatorId { get; set; }

        public User? Annotator { get; set; }

        public DatasetRound Round { get; set; } = null!;
    }
}