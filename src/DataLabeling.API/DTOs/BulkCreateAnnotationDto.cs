namespace DataLabeling.API.DTOs
{
    public class BulkCreateAnnotationDto
    {
        public int TaskId { get; set; }

        public int RoundId { get; set; }

        public List<CreateAnnotationItemDto> Items { get; set; } = new();
    }
}

