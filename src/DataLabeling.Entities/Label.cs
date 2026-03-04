using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Label
    {
        [Key]
        public int LabelId { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Required]
        public string LabelName { get; set; } = string.Empty;

        public string? LabelType { get; set; }

        public string? Description { get; set; }

        public Project Project { get; set; } = null!;
    }
}
