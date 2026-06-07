namespace GeneFlow.Core.DTOs.Auth;

public class LoginRequest
{
    // Either Email or PhoneNumber must be provided
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class RegisterLabRequest
{
    public string LabName { get; set; } = string.Empty;
    public string? InstitutionName { get; set; }
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string? AdminPhoneNumber { get; set; }
    public string AdminPassword { get; set; } = string.Empty;
}

public class AddLabUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string LabRole { get; set; } = "Researcher";
}

public class RegisterLabResponse
{
    public Guid LabId { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class ForgotPasswordRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ForgotPasswordResponse
{
    /// <summary>
    /// One-time 6-digit code. In production this would be sent via SMS/email.
    /// Returned in response for now (closed lab app, no SMS service yet).
    /// </summary>
    public string ResetCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string ResetCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AdminResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
