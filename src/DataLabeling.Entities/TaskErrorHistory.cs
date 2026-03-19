using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLabeling.Entities
{
    public class TaskErrorHistory
    {
        [Key]
        public int ErrorId { get; set; }

        public int TaskId { get; set; }

        [ForeignKey("DataItem")]
        public int ItemId { get; set; }

        public int ReviewerId { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Task Task { get; set; } = null!;

        public DataItem DataItem { get; set; } = null!;

        public User Reviewer { get; set; } = null!;
    }
}