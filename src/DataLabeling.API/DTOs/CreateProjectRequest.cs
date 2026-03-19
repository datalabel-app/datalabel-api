namespace DataLabeling.API.DTOs
{
    public class CreateProjectRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
