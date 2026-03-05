namespace DataLabeling.API.DTOs
{
    public class CreateLabelRequest
    {
        public int ProjectId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string? LabelType { get; set; }
        public string? Description { get; set; }
    }
}
