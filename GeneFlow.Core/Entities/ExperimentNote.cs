using GeneFlow.Core.Enums;

namespace GeneFlow.Core.Entities;

public class ExperimentNote
{
    public Guid ExperimentNoteId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid UserId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public NoteType NoteType { get; set; } = NoteType.General;
    public DateTime CreatedAt { get; set; }

    public Experiment Experiment { get; set; } = null!;
    public User User { get; set; } = null!;
}
