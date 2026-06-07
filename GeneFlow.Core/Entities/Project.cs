namespace GeneFlow.Core.Entities;

public class Project
{
    public Guid ProjectId { get; set; }
    public Guid LabId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Lab Lab { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    public ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();
}
