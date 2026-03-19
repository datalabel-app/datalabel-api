namespace DataLabeling.API.DTOs
{
    public class ProjectResponseAll
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string ManagerName { get; set; } = string.Empty;

        public int DatasetCount { get; set; }
    }
}
