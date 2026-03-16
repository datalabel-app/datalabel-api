using DataLabeling.Entities;
using System.ComponentModel.DataAnnotations;

namespace DataLabeling.API.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        public int DataItemId { get; set; }

        [Required]
        public int RoundId { get; set; }

        public int? AnnotatorId { get; set; }

        public int? ReviewerId { get; set; }
    }
}