namespace GeneFlow.Core.Entities;

public class RawCtData
{
    public Guid RawCtDataId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid UploadedFileId { get; set; }
    public string WellId { get; set; } = string.Empty;
    public string? SampleNameFromFile { get; set; }
    public string? TargetGeneFromFile { get; set; }
    public string? CtValueRaw { get; set; }
    public decimal? CtValue { get; set; }
    public bool IsValid { get; set; } = true;
    public string? ValidationMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public UploadedFile UploadedFile { get; set; } = null!;
}
