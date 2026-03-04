using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class DataItem
    {
        [Key]
        public int ItemId { get; set; }

        [ForeignKey("Dataset")]
        public int DatasetId { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public DataItemStatus Status { get; set; } = DataItemStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dataset Dataset { get; set; } = null!;
    }
}
