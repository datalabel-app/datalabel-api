using DataLabeling.Entities;
using System.ComponentModel.DataAnnotations;

namespace DataLabeling.API.DTOs
{
    public class CreateTaskRequest
    {
        public int RoundId { get; set; }
        public int? AnnotatorId { get; set; }
        public int? ReviewerId { get; set; }

        public DateTime? Deadline { get; set; }

        public List<int> DataItemIds { get; set; } = new();
    }
}