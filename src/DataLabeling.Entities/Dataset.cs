using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Dataset
    {
        [Key]
        public int DatasetId { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Required]
        public string DatasetName { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;

        public ICollection<DatasetRound> DatasetRounds { get; set; } = new List<DatasetRound>();

        public ICollection<DataItem> DataItems { get; set; } = new List<DataItem>();
    }
}
