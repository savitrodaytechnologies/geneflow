namespace GeneFlow.Core.Entities;

public class PlateWell
{
    public Guid PlateWellId { get; set; }
    public Guid PlateLayoutId { get; set; }
    public Guid ExperimentId { get; set; }
    public string WellId { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public int ColumnNumber { get; set; }
    public string? SampleName { get; set; }
    public string? TargetGene { get; set; }
    public string? ReferenceGene { get; set; }
    public string? SampleType { get; set; }
    public string? ReplicateGroup { get; set; }
    public decimal? CtValue { get; set; }
    public bool IsExcluded { get; set; } = false;
    public string? ExclusionReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public PlateLayout PlateLayout { get; set; } = null!;
    public Experiment Experiment { get; set; } = null!;
}
