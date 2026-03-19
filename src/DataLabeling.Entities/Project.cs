using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLabeling.Entities
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        public int ManagerId { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Manager { get; set; } = null!;

        public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
    }
}