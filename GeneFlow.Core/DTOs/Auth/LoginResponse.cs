namespace GeneFlow.Core.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SystemRole { get; set; } = string.Empty;
    public Guid? LabId { get; set; }
    public string? LabRole { get; set; }
}
