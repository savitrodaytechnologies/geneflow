namespace GeneFlow.Core.Entities;

public class AnalysisResult
{
    public Guid AnalysisResultId { get; set; }
    public Guid AnalysisRunId { get; set; }
    public Guid ExperimentId { get; set; }
    public string SampleName { get; set; } = string.Empty;
    public string TargetGene { get; set; } = string.Empty;
    public string ReferenceGene { get; set; } = string.Empty;
    public string? SampleType { get; set; }
    public decimal? MeanTargetCt { get; set; }
    public decimal? SdTargetCt { get; set; }
    public decimal? MeanReferenceCt { get; set; }
    public decimal? SdReferenceCt { get; set; }
    public decimal? DeltaCt { get; set; }
    public decimal? DeltaDeltaCt { get; set; }
    public decimal? FoldChange { get; set; }
    public decimal? Log2FoldChange { get; set; }
    public int TargetReplicateCount { get; set; } = 0;
    public int ReferenceReplicateCount { get; set; } = 0;
    public bool HasWarning { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public AnalysisRun AnalysisRun { get; set; } = null!;
    public Experiment Experiment { get; set; } = null!;
    public ICollection<AnalysisWarning> AnalysisWarnings { get; set; } = new List<AnalysisWarning>();
}
