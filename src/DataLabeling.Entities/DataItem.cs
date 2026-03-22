using DataLabeling.Entities;
using System.ComponentModel.DataAnnotations;

public class DataItem
{
    [Key]
    public int ItemId { get; set; }

    public int DatasetId { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Dataset Dataset { get; set; } = null!;

    public ICollection<TaskDataItem> TaskDataItems { get; set; } = new List<TaskDataItem>();

    public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public ICollection<TaskErrorHistory> ErrorHistories { get; set; } = new List<TaskErrorHistory>();


}