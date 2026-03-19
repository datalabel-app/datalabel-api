namespace DataLabeling.DTOs.Annotations
{
    public class AnnotationResponse
    {
        public int AnnotationId { get; set; }

        public int TaskId { get; set; }

        public int LabelId { get; set; }

        public string ShapeType { get; set; }

        public string Coordinates { get; set; }

        public string? Classification { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}