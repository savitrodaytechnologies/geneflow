namespace GeneFlow.Core.DTOs.Dashboard;

public class MobileDashboardDto
{
    public DashboardSummaryDto Summary { get; set; } = null!;
    public List<RecentExperimentDto> RecentExperiments { get; set; } = new();
}

public class DashboardSummaryDto
{
    public int MyDrafts { get; set; }
    public int PendingAnalysis { get; set; }
    public int ExperimentsWithWarnings { get; set; }
    public int RecentReports { get; set; }
}

public class RecentExperimentDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentName { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int WarningCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
