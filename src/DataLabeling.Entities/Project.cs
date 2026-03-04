using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [ForeignKey("Manager")]
        public int ManagerId { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Manager { get; set; } = null!;

        public ICollection<Label> Labels { get; set; } = new List<Label>();

        public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
    }
}
