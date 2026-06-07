using GeneFlow.Core.Enums;

namespace GeneFlow.Core.Entities;

public class LabUser
{
    public Guid LabUserId { get; set; }
    public Guid LabId { get; set; }
    public Guid UserId { get; set; }
    public LabRole LabRole { get; set; } = LabRole.Researcher;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }

    public Lab Lab { get; set; } = null!;
    public User User { get; set; } = null!;
}
