namespace DataLabeling.API.DTOs
{
    public class BulkReviewDto
    {
        public int TaskId { get; set; }

        public Dictionary<int, ReviewDto> Items { get; set; } = new();
    }
}
