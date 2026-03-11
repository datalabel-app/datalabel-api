using DataLabeling.Entities.Enums;

namespace DataLabeling.API.DTOs
{
    public class DatasetRoundResponse
    {
        public int DatasetRoundId { get; set; }
        public int DatasetId { get; set; }
        public int RoundId { get; set; }
        public DatasetRoundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}