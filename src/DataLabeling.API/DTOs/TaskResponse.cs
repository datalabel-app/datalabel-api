using DataLabeling.Entities;
namespace DataLabeling.API.DTOs
{
    public class TaskResponse
    {
        public int TaskId { get; set; }
        public int RoundId { get; set; }
        public int? AnnotatorId { get; set; }
        public int? ReviewerId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<int> DataItemIds { get; set; } = new();
    }
}