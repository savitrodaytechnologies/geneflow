namespace GeneFlow.Core.DTOs.Lab;

public class LabDto
{
    public Guid LabId { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? InstitutionName { get; set; }
    public string? Description { get; set; }
}

public class LabMemberDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string LabRole { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
