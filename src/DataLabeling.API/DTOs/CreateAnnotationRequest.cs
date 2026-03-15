using System.ComponentModel.DataAnnotations;

namespace DataLabeling.DTOs.Annotations
{
    public class CreateAnnotationRequest
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public int LabelId { get; set; }

        [Required]
        public int RoundId { get; set; }


        public string ShapeType { get; set; } = "bbox";

        public string Coordinates { get; set; } = string.Empty;

        public string? Classification { get; set; }
    }
}