namespace DataLabeling.API.DTOs
{
    public class CreateLabelRequest
    {
        public int RoundId { get; set; }

        public string LabelName { get; set; }

        public string? Description { get; set; }

        public int ProjectId { get; set; }
        public int? ParentDatasetId { get; set; }
    }
}
