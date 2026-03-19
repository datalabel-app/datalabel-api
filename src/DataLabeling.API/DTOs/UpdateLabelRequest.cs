namespace DataLabeling.API.DTOs
{
    public class UpdateLabelRequest
    {
        public string LabelName { get; set; } = string.Empty;
        public string? LabelType { get; set; }
        public string? Description { get; set; }
    }
}
