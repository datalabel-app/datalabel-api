namespace DataLabeling.API.DTOs
{
    public class DatasetTreeResponse
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ShapeType { get; set; }
        public List<DatasetTreeResponse> Children { get; set; } = new();
    }
}
