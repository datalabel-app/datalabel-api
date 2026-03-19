namespace DataLabeling.API.DTOs
{
    public class UpdateProjectRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
    }
}
