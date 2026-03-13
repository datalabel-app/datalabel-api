using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class UpdateTaskRequest
    {
        public DataLabeling.Entities.TaskStatus? Status { get; set; }
        //public TaskType? Type { get; set; }
        public int? GroupNumber { get; set; }
    }
}