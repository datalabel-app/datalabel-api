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

        [ForeignKey("DatasetRound")]
        public int DatasetRoundId { get; set; }

        [ForeignKey("AssigneeUser")]
        public int AssigneeUserId { get; set; }

        public TaskType Type { get; set; } = TaskType.Labeling;

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public int GroupNumber { get; set; }

        public int? ParentTaskId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }


        public DatasetRound DatasetRound { get; set; } = null!;

        public User AssigneeUser { get; set; } = null!;

        public Task? ParentTask { get; set; }

        public ICollection<Task> SubTasks { get; set; } = new List<Task>();
    }
}
