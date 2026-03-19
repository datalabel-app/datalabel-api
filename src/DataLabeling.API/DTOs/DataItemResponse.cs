using DataLabeling.Entities;
using DataLabeling.Entities.Enums;

namespace DataLabeling.API.DTOs
{
    public class DataItemResponse
    {
        public int ItemId { get; set; }

        public int DatasetId { get; set; }

        public string FileUrl { get; set; } = string.Empty;


    
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}