using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class DataItemResponse
    {
        public int ItemId { get; set; }
        public int DatasetId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DataItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}