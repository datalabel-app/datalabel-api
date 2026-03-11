namespace DataLabeling.API.DTOs
{
    public class CreateRoundRequest
    {
        public int DatasetId { get; set; }

        public int RoundNumber { get; set; }

        public string? Description { get; set; }
    }
}
