namespace GeneFlow.Core.Entities;

public class UploadedFile
{
    public Guid UploadedFileId { get; set; }
    public Guid ExperimentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;
    public ICollection<RawCtData> RawCtData { get; set; } = new List<RawCtData>();
}
