using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class UpdateTaskRequest
    {
        public int? AnnotatorId { get; set; }
        public int? ReviewerId { get; set; }
        public DataLabeling.Entities.TaskStatus? Status { get; set; }
        public string? DescriptionError { get; set; }
    }
}