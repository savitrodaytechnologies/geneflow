namespace GeneFlow.Core.Entities;

public class ProjectUser
{
    public Guid ProjectUserId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string ProjectRole { get; set; } = "Member";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
