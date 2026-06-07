namespace GeneFlow.Core.Entities;

public class ExportFile
{
    public Guid ExportFileId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid? AnalysisRunId { get; set; }
    public string ExportType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public AnalysisRun? AnalysisRun { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
