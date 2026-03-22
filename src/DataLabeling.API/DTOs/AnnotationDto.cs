namespace DataLabeling.API.DTOs
{
    public class AnnotationDto
    {
        public int AnnotationId { get; set; }
        public int LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;

        public int AnnotatorId { get; set; }
        public string AnnotatorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
