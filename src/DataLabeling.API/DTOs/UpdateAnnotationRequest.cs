namespace DataLabeling.API.DTOs
{
    public class UpdateAnnotationRequest
    {
        public int? LabelId { get; set; }

        public string? Coordinates { get; set; }

        public string? Classification { get; set; }
    }
}
