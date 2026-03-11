

using DataLabeling.Entities;

public class CreateDatasetRoundRequest
{
    public int DatasetId { get; set; }
    public int RoundId { get; set; }
    public DatasetRoundStatus Status { get; set; } = DatasetRoundStatus.Pending;
}