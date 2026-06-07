using GeneFlow.Core.Enums;

namespace GeneFlow.Core.DTOs.Experiment;

public class ExperimentSummaryDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentName { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public int WarningCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateOnly ExperimentDate { get; set; }
}

public class ExperimentDetailDto
{
    public Guid ExperimentId { get; set; }
    public Guid LabId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string ExperimentName { get; set; } = string.Empty;
    public DateOnly ExperimentDate { get; set; }
    public string? Objective { get; set; }
    public string? Hypothesis { get; set; }
    public string? SampleSource { get; set; }
    public string? TreatmentCondition { get; set; }
    public string? InstrumentName { get; set; }
    public string? ReferenceGene { get; set; }
    public string? ControlSampleName { get; set; }
    public string PlateType { get; set; } = "96-well";
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateExperimentRequest
{
    public Guid? ProjectId { get; set; }
    public string ExperimentName { get; set; } = string.Empty;
    public DateOnly ExperimentDate { get; set; }
    public string? Objective { get; set; }
    public string? Hypothesis { get; set; }
    public string? SampleSource { get; set; }
    public string? TreatmentCondition { get; set; }
    public string? InstrumentName { get; set; }
    public string? ReferenceGene { get; set; }
    public string? ControlSampleName { get; set; }
    public ExperimentVisibility Visibility { get; set; } = ExperimentVisibility.Lab;
    public string? Notes { get; set; }
}

public class UpdateExperimentRequest
{
    public string ExperimentName { get; set; } = string.Empty;
    public DateOnly ExperimentDate { get; set; }
    public string? Objective { get; set; }
    public string? Hypothesis { get; set; }
    public string? SampleSource { get; set; }
    public string? TreatmentCondition { get; set; }
    public string? InstrumentName { get; set; }
    public string? ReferenceGene { get; set; }
    public string? ControlSampleName { get; set; }
    public ExperimentVisibility Visibility { get; set; } = ExperimentVisibility.Lab;
    public string? Notes { get; set; }
}
