namespace DataLabeling.API.DTOs
{
    public class CreateLabelRequest
    {
        public int RoundId { get; set; }

        public string LabelName { get; set; }

        public string? Description { get; set; }
    }
}
