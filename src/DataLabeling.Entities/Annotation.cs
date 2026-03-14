using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Annotation
    {
        [Key]
        public int AnnotationId { get; set; }

        public int ItemId { get; set; }

        public int LabelId { get; set; }

        public int RoundId { get; set; }

        public int AnnotatorId { get; set; }

        public string ShapeType { get; set; } = "bbox";

        public string Coordinates { get; set; } = string.Empty;

        public string? Classification { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DataItem DataItem { get; set; } = null!;

        public Label Label { get; set; } = null!;

        public User Annotator { get; set; } = null!;
    }
}
