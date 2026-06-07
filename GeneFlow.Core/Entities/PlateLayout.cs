namespace GeneFlow.Core.Entities;

public class PlateLayout
{
    public Guid PlateLayoutId { get; set; }
    public Guid ExperimentId { get; set; }
    public string PlateType { get; set; } = "96-well";
    public int RowCount { get; set; } = 8;
    public int ColumnCount { get; set; } = 12;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public ICollection<PlateWell> PlateWells { get; set; } = new List<PlateWell>();
}
