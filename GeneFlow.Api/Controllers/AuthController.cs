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
}

