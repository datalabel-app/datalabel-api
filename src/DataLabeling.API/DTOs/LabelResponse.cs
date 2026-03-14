namespace DataLabeling.API.DTOs
{
    public class LabelResponse
    {
        public int LabelId { get; set; }

        public int RoundId { get; set; }

        public string LabelName { get; set; }

        public string? Description { get; set; }
    }
}
