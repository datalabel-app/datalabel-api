namespace DataLabeling.API.DTOs
{
    public class DatasetDetailDto
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string Status { get; set; }

        public ProjectDto Project { get; set; }
    }
}
