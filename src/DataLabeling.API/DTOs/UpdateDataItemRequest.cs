using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class UpdateDataItemRequest
    {
        public DataItemStatus? Status { get; set; }
        public string? FileUrl { get; set; }
    }
}