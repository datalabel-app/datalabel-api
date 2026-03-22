using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        public int RoundId { get; set; }

        public int? AnnotatorId { get; set; }

        public int? ReviewerId { get; set; }

        public string? DescriptionError { get; set; } = string.Empty;

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AnnotatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DatasetRound Round { get; set; } = null!;

        public User? Annotator { get; set; }

        public User? Reviewer { get; set; }

        public ICollection<TaskDataItem> TaskDataItems { get; set; } = new List<TaskDataItem>();

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

        public ICollection<TaskErrorHistory> ErrorHistories { get; set; } = new List<TaskErrorHistory>();
    }
}
