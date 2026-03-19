using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class UpdateTaskRequest
    {
        public int? AnnotatorId { get; set; }

        public int? ReviewerId { get; set; }

        public string? Status { get; set; }

        public string? DescriptionError { get; set; }
    }
}