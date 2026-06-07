using GeneFlow.Core.Enums;

namespace GeneFlow.Core.Entities;

public class Experiment
{
    public Guid ExperimentId { get; set; }
    public Guid LabId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid OwnerUserId { get; set; }
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
    public ExperimentStatus Status { get; set; } = ExperimentStatus.Draft;
    public ExperimentVisibility Visibility { get; set; } = ExperimentVisibility.Lab;
    public string? Notes { get; set; }
    public DateTime? LockedAt { get; set; }
    public Guid? LockedByUserId { get; set; }
    public bool IsArchived { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Lab Lab { get; set; } = null!;
    public Project? Project { get; set; }
    public User OwnerUser { get; set; } = null!;
    public ICollection<PlateLayout> PlateLayouts { get; set; } = new List<PlateLayout>();
    public ICollection<PlateWell> PlateWells { get; set; } = new List<PlateWell>();
    public ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
    public ICollection<AnalysisRun> AnalysisRuns { get; set; } = new List<AnalysisRun>();
    public ICollection<ExperimentNote> ExperimentNotes { get; set; } = new List<ExperimentNote>();
}
