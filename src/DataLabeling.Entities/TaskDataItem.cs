using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class TaskDataItem
    {
        public int TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public int DataItemId { get; set; }
        public DataItem DataItem { get; set; } = null!;

        public string ReviewStatus { get; set; } = "Pending"; // Pending | Approved | Rejected | Annotating
        public string? ReviewComment { get; set; }

        public int? ReviewerId { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
