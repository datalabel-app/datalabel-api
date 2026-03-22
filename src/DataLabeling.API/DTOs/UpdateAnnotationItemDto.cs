namespace DataLabeling.API.DTOs
{
    public class UpdateAnnotationItemDto
    {
        public int ItemId { get; set; }
        public int AnnotationId { get; set; }
        public int LabelId { get; set; }
        public string Classification { get; set; } = string.Empty;
    }
}
