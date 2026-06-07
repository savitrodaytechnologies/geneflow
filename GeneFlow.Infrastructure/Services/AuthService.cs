using GeneFlow.Core.DTOs.Auth;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly GeneFlowDbContext _db;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(GeneFlowDbContext db, JwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() && u.IsActive);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var labUser = await _db.LabUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.UserId == user.UserId && lu.IsActive);

        var token = _jwtTokenService.GenerateToken(
            user.UserId,
            user.Email,
            user.SystemRole,
            labUser?.LabId,
            labUser?.LabRole.ToString());

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                SystemRole = user.SystemRole,
                LabId = labUser?.LabId,
                LabRole = labUser?.LabRole.ToString()
            }
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

        if (user is null) return null;

        var labUser = await _db.LabUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.UserId == userId && lu.IsActive);

        return new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            SystemRole = user.SystemRole,
            LabId = labUser?.LabId,
            LabRole = labUser?.LabRole.ToString()
        };
    }
}
