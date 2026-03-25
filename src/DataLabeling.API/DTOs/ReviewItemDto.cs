namespace DataLabeling.API.DTOs
{
    public class ReviewItemDto
    {
        public int ItemId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = null!;
        public string? ReviewComment { get; set; }
        public AnnotationDto? Annotation { get; set; }
    }
}
