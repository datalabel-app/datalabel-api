using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLabeling.Entities
{
    public class Dataset
    {
        [Key]
        public int DatasetId { get; set; }

        public int? ParentDatasetId { get; set; }

        public int? LabelId { get; set; }

        public int ProjectId { get; set; }

        public string DatasetName { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ParentDatasetId")]
        public Dataset? ParentDataset { get; set; }



        [ForeignKey("LabelId")]
        public Label? Label { get; set; }
        public ICollection<Dataset> SubDatasets { get; set; } = new List<Dataset>();

        public Project Project { get; set; } = null!;

        public ICollection<DataItem> DataItems { get; set; } = new List<DataItem>();

        public ICollection<DatasetRound> Rounds { get; set; } = new List<DatasetRound>();
    }
}