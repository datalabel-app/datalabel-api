namespace DataLabeling.API.DTOs
{
    public class BulkUpdateAnnotationDto
    {
        public int TaskId { get; set; }
        public int RoundId { get; set; }
        public List<UpdateAnnotationItemDto> Items { get; set; } = new();
    }
}
