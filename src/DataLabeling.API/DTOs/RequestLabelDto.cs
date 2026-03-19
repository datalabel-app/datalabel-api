using System.ComponentModel.DataAnnotations;

namespace DataLabeling.DTOs.Labels
{
    public class RequestLabelDto
    {
        [Required]
        public int RoundId { get; set; }

        [Required]
        public string LabelName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}