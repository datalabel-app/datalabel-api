namespace DataLabeling.API.DTOs
{
    public class CreateDatasetRequest
    {
        public int ProjectId { get; set; }
        public string DatasetName { get; set; } = string.Empty;
        public string? Status { get; set; }
    }
}
