namespace DataLabeling.API.DTOs
{
    public class DatasetResponse
    {
        public int DatasetId { get; set; }
        public int ProjectId { get; set; }
        public string DatasetName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
