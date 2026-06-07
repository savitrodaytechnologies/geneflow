using GeneFlow.Core.DTOs.Auth;
using GeneFlow.Core.DTOs.Lab;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
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

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        User? user = null;

        // Try phone number first, then email
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var normalizedPhone = NormalizePhone(request.PhoneNumber);
            user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone && u.IsActive);
        }

        if (user is null && !string.IsNullOrWhiteSpace(request.Email))
        {
            user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive);
        }

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var labUser = await _db.LabUsers.AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.UserId == user.UserId && lu.IsActive);

        var token = _jwtTokenService.GenerateToken(
            user.UserId, user.Email, user.SystemRole,
            labUser?.LabId, labUser?.LabRole.ToString());

        return new LoginResponse
        {
            Token = token,
            User = MapUserDto(user, labUser)
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

        if (user is null) return null;

        var labUser = await _db.LabUsers.AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.UserId == userId && lu.IsActive);

        return MapUserDto(user, labUser);
    }

    public async Task<RegisterLabResponse> RegisterLabAsync(RegisterLabRequest request)
    {
        // Validate uniqueness
        var normalizedEmail = request.AdminEmail.ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.AdminPhoneNumber)
            ? null : NormalizePhone(request.AdminPhoneNumber);

        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail))
            throw new InvalidOperationException("Email already registered.");

        if (normalizedPhone != null && await _db.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone))
            throw new InvalidOperationException("Phone number already registered.");

        var lab = new Lab
        {
            LabId = Guid.NewGuid(),
            LabName = request.LabName.Trim(),
            InstitutionName = request.InstitutionName?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone,
            FullName = request.AdminFullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            SystemRole = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var labUser = new LabUser
        {
            LabUserId = Guid.NewGuid(),
            LabId = lab.LabId,
            UserId = user.UserId,
            LabRole = LabRole.LabAdmin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        _db.Labs.Add(lab);
        _db.Users.Add(user);
        _db.LabUsers.Add(labUser);
        await _db.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(
            user.UserId, user.Email, user.SystemRole,
            lab.LabId, LabRole.LabAdmin.ToString());

        return new RegisterLabResponse
        {
            LabId = lab.LabId,
            LabName = lab.LabName,
            Token = token,
            User = MapUserDto(user, labUser)
        };
    }

    public async Task<LabMemberDto> AddLabUserAsync(Guid labId, Guid requestingUserId, AddLabUserRequest request)
    {
        // Only LabAdmin of this lab can add users
        var requesterLabUser = await _db.LabUsers
            .FirstOrDefaultAsync(lu => lu.LabId == labId && lu.UserId == requestingUserId && lu.IsActive);
        if (requesterLabUser is null || requesterLabUser.LabRole != LabRole.LabAdmin)
            throw new UnauthorizedAccessException("Only a Lab Admin can add users.");

        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null : request.Email.ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null : NormalizePhone(request.PhoneNumber);

        if (normalizedEmail is null && normalizedPhone is null)
            throw new InvalidOperationException("Either email or phone number is required.");

        // Find existing user or create new one
        User? user = null;
        if (normalizedPhone != null)
            user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
        if (user is null && normalizedEmail != null)
            user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
        {
            user = new User
            {
                UserId = Guid.NewGuid(),
                Email = normalizedEmail ?? $"{Guid.NewGuid():N}@noemail.local",
                PhoneNumber = normalizedPhone,
                FullName = request.FullName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                SystemRole = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
        }

        // Check if already a member of this lab
        var existing = await _db.LabUsers
            .FirstOrDefaultAsync(lu => lu.LabId == labId && lu.UserId == user.UserId);
        if (existing != null)
        {
            if (existing.IsActive)
                throw new InvalidOperationException("User is already a member of this lab.");
            existing.IsActive = true;
            existing.LabRole = Enum.TryParse<LabRole>(request.LabRole, out var r) ? r : LabRole.Researcher;
        }
        else
        {
            var labUser = new LabUser
            {
                LabUserId = Guid.NewGuid(),
                LabId = labId,
                UserId = user.UserId,
                LabRole = Enum.TryParse<LabRole>(request.LabRole, out var r2) ? r2 : LabRole.Researcher,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };
            _db.LabUsers.Add(labUser);
        }

        await _db.SaveChangesAsync();

        var lu = await _db.LabUsers.AsNoTracking()
            .FirstAsync(x => x.LabId == labId && x.UserId == user.UserId);

        return new LabMemberDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            LabRole = lu.LabRole.ToString(),
            IsActive = lu.IsActive
        };
    }

    public async Task<List<LabMemberDto>> GetLabMembersAsync(Guid labId)
    {
        return await _db.LabUsers
            .AsNoTracking()
            .Where(lu => lu.LabId == labId)
            .Select(lu => new LabMemberDto
            {
                UserId = lu.User.UserId,
                FullName = lu.User.FullName,
                Email = lu.User.Email,
                PhoneNumber = lu.User.PhoneNumber,
                LabRole = lu.LabRole.ToString(),
                IsActive = lu.IsActive
            })
            .ToListAsync();
    }

    public async Task<bool> DeactivateLabUserAsync(Guid labId, Guid targetUserId, Guid requestingUserId)
    {
        var requester = await _db.LabUsers
            .FirstOrDefaultAsync(lu => lu.LabId == labId && lu.UserId == requestingUserId && lu.IsActive);
        if (requester is null || requester.LabRole != LabRole.LabAdmin)
            throw new UnauthorizedAccessException("Only a Lab Admin can remove users.");

        var target = await _db.LabUsers
            .FirstOrDefaultAsync(lu => lu.LabId == labId && lu.UserId == targetUserId);
        if (target is null) return false;

        target.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Forgot / Reset password ────────────────────────────────────────────

    // In-memory store: normalizedIdentifier → (code, expiry)
    // Acceptable for a closed-lab app; survives restarts fine for short-lived codes.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Code, DateTime Expiry)>
        _resetTokens = new();

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == NormalizePhone(request.PhoneNumber) && u.IsActive);

        if (user is null && !string.IsNullOrWhiteSpace(request.Email))
            user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive);

        if (user is null) return null; // Don't reveal whether account exists

        var code = Random.Shared.Next(100_000, 999_999).ToString();
        var key = user.UserId.ToString();
        _resetTokens[key] = (code, DateTime.UtcNow.AddMinutes(15));

        return new ForgotPasswordResponse
        {
            ResetCode = code,
            Message = $"Reset code generated for {user.FullName}. Valid for 15 minutes."
        };
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await _db.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == NormalizePhone(request.PhoneNumber) && u.IsActive);

        if (user is null && !string.IsNullOrWhiteSpace(request.Email))
            user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive);

        if (user is null) return false;

        var key = user.UserId.ToString();
        if (!_resetTokens.TryGetValue(key, out var entry)) return false;
        if (entry.Expiry < DateTime.UtcNow || entry.Code != request.ResetCode.Trim()) return false;

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _resetTokens.TryRemove(key, out _);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
        if (user is null) return false;

        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new InvalidOperationException("New password must be at least 8 characters.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string> AdminResetPasswordAsync(Guid labId, Guid targetUserId, Guid requestingUserId, AdminResetPasswordRequest request)
    {
        var requester = await _db.LabUsers
            .FirstOrDefaultAsync(lu => lu.LabId == labId && lu.UserId == requestingUserId && lu.IsActive);
        if (requester is null || requester.LabRole != LabRole.LabAdmin)
            throw new UnauthorizedAccessException("Only a Lab Admin can reset passwords.");

        var target = await _db.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId && u.IsActive);
        if (target is null) throw new InvalidOperationException("User not found.");

        // Verify target is in this lab
        var targetLabUser = await _db.LabUsers.AnyAsync(lu => lu.LabId == labId && lu.UserId == targetUserId);
        if (!targetLabUser) throw new UnauthorizedAccessException("User is not a member of this lab.");

        var newPassword = string.IsNullOrWhiteSpace(request.NewPassword)
            ? GenerateTempPassword()
            : request.NewPassword;

        if (newPassword.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");

        target.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        target.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return newPassword;
    }

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#";
        var rng = new char[10];
        for (int i = 0; i < rng.Length; i++)
            rng[i] = chars[Random.Shared.Next(chars.Length)];
        return new string(rng);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static string NormalizePhone(string phone) =>
        new string(phone.Where(char.IsDigit).ToArray());

    private static UserDto MapUserDto(User user, LabUser? labUser) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        FullName = user.FullName,
        SystemRole = user.SystemRole,
        LabId = labUser?.LabId,
        LabRole = labUser?.LabRole.ToString()
    };
}

