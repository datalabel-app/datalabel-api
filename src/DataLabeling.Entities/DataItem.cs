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

        public int DatasetId { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public int? AnnotatorId { get; set; }

        public int? ReviewerId { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dataset Dataset { get; set; } = null!;

        public User? Annotator { get; set; }

        public User? Reviewer { get; set; }

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}
