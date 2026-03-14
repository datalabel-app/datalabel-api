namespace DataLabeling.API.DTOs
{
    public class ProjectResponse
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public ManagerResponse Manager { get; set; }

        public List<DatasetResponse> Datasets { get; set; }
    }
}
