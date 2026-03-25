namespace DataLabeling.API.DTOs
{
    public class CreateAnnotationItemDto
    {
        public int ItemId { get; set; }

        public int LabelId { get; set; }

        public string? Classification { get; set; }
    }
}
