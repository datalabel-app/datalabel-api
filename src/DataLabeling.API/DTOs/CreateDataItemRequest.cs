using DataLabeling.Entities;
using DataLabeling.Entities.Enums;

namespace DataLabeling.API.DTOs
{
    public class CreateDataItemRequest
    {
        public int DatasetId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DataItemStatus Status { get; set; } = DataItemStatus.Pending;
    }
}