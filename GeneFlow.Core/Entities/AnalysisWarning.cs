using GeneFlow.Core.Enums;

namespace GeneFlow.Core.Entities;

public class AnalysisWarning
{
    public Guid AnalysisWarningId { get; set; }
    public Guid AnalysisRunId { get; set; }
    public Guid? AnalysisResultId { get; set; }
    public Guid? PlateWellId { get; set; }
    public string WarningType { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public WarningSeverity Severity { get; set; } = WarningSeverity.Warning;
    public DateTime CreatedAt { get; set; }

    public AnalysisRun AnalysisRun { get; set; } = null!;
    public AnalysisResult? AnalysisResult { get; set; }
}
