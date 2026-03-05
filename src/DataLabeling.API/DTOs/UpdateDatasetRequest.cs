namespace DataLabeling.API.DTOs
{
    public class UpdateDatasetRequest
    {
        public string DatasetName { get; set; } = string.Empty;
        public string? Status { get; set; }
    }
}
