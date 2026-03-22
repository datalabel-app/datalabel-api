namespace DataLabeling.API.DTOs
{
    public class ReviewTaskResponseDto
    {
        public int TaskId { get; set; }
        public int RoundId { get; set; }
        public string Status { get; set; } = string.Empty;

        public List<ReviewItemDto> Items { get; set; } = new();
    }
}
