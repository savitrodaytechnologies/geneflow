namespace GeneFlow.Core.Entities;

public class AnalysisRun
{
    public Guid AnalysisRunId { get; set; }
    public Guid ExperimentId { get; set; }
    public string? RunName { get; set; }
    public string ControlSampleName { get; set; } = string.Empty;
    public string ReferenceGene { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = "DeltaDeltaCt";
    public decimal HighCtThreshold { get; set; } = 35m;
    public decimal ReplicateSdThreshold { get; set; } = 0.5m;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
    public ICollection<AnalysisWarning> AnalysisWarnings { get; set; } = new List<AnalysisWarning>();
}
