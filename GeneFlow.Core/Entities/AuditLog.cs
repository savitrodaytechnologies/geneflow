namespace GeneFlow.Core.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid? LabId { get; set; }
    public Guid? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
