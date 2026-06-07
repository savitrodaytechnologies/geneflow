using GeneFlow.Core.DTOs.Auth;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly CurrentUserService _currentUser;

    public AuthController(IAuthService authService, CurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required." });
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "Email or phone number is required." });

        var result = await _authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = "Invalid credentials." });

        return Ok(result);
    }

    [HttpPost("register-lab")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterLab([FromBody] RegisterLabRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LabName))
            return BadRequest(new { message = "Lab name is required." });
        if (string.IsNullOrWhiteSpace(request.AdminFullName))
            return BadRequest(new { message = "Admin full name is required." });
        if (string.IsNullOrWhiteSpace(request.AdminEmail))
            return BadRequest(new { message = "Admin email is required." });
        if (string.IsNullOrWhiteSpace(request.AdminPassword) || request.AdminPassword.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        try
        {
            var result = await _authService.RegisterLabAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _authService.GetCurrentUserAsync(_currentUser.UserId);
        if (user is null) return Unauthorized();
        return Ok(user);
    }

    // ── Lab user management ────────────────────────────────────────────────
    [HttpGet("labs/{labId}/members")]
    [Authorize]
    public async Task<IActionResult> GetLabMembers(Guid labId)
    {
        var members = await _authService.GetLabMembersAsync(labId);
        return Ok(members);
    }

    [HttpPost("labs/{labId}/members")]
    [Authorize]
    public async Task<IActionResult> AddLabMember(Guid labId, [FromBody] AddLabUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Full name is required." });
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "Email or phone number is required." });
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        try
        {
            var member = await _authService.AddLabUserAsync(labId, _currentUser.UserId, request);
            return Ok(member);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("labs/{labId}/members/{userId}")]
    [Authorize]
    public async Task<IActionResult> RemoveLabMember(Guid labId, Guid userId)
    {
        try
        {
            var ok = await _authService.DeactivateLabUserAsync(labId, userId, _currentUser.UserId);
            return ok ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // ── Password management ────────────────────────────────────────────────

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "Email or phone number is required." });

        // Always return 200 to avoid account enumeration; result is null if user not found
        var result = await _authService.ForgotPasswordAsync(request);
        if (result is null)
            return Ok(new { message = "If that account exists, a reset code has been provided." });

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResetCode))
            return BadRequest(new { message = "Reset code is required." });
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        try
        {
            var ok = await _authService.ResetPasswordAsync(request);
            if (!ok) return BadRequest(new { message = "Invalid or expired reset code." });
            return Ok(new { message = "Password reset successfully. Please log in." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return BadRequest(new { message = "Current password is required." });
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        try
        {
            var ok = await _authService.ChangePasswordAsync(_currentUser.UserId, request);
            if (!ok) return BadRequest(new { message = "Current password is incorrect." });
            return Ok(new { message = "Password changed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("labs/{labId}/members/{userId}/reset-password")]
    [Authorize]
    public async Task<IActionResult> AdminResetPassword(Guid labId, Guid userId, [FromBody] AdminResetPasswordRequest request)
    {
        try
        {
            var newPassword = await _authService.AdminResetPasswordAsync(labId, userId, _currentUser.UserId, request);
            return Ok(new { message = "Password reset successfully.", temporaryPassword = newPassword });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

