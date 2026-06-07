using FluentAssertions;
using GeneFlow.Core.DTOs.Auth;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Infrastructure.Services;
using GeneFlow.Tests.Helpers;
using Xunit;

namespace GeneFlow.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(GeneFlow.Infrastructure.Data.GeneFlowDbContext db)
        => new AuthService(db, TestFactory.CreateJwtService());

    // ─── LOGIN ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidEmail_ReturnsToken()
    {
        using var db = TestFactory.CreateDbContext();
        await SeedUserAsync(db);
        var svc = CreateService(db);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email = "lab@test.com",
            Password = "Password1!"
        });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("lab@test.com");
    }

    [Fact]
    public async Task Login_WithValidPhone_ReturnsToken()
    {
        using var db = TestFactory.CreateDbContext();
        await SeedUserAsync(db);
        var svc = CreateService(db);

        // Phone stored as digits only; login sends digits
        var result = await svc.LoginAsync(new LoginRequest
        {
            PhoneNumber = "19876543210",
            Password = "Password1!"
        });

        result.Should().NotBeNull();
        result!.User.PhoneNumber.Should().Be("19876543210");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        using var db = TestFactory.CreateDbContext();
        await SeedUserAsync(db);
        var svc = CreateService(db);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email = "lab@test.com",
            Password = "WrongPassword!"
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsNull()
    {
        using var db = TestFactory.CreateDbContext();
        var svc = CreateService(db);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email = "nobody@test.com",
            Password = "Password1!"
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_IncludesLabIdInToken_WhenUserHasLab()
    {
        using var db = TestFactory.CreateDbContext();
        var (userId, labId) = await SeedUserWithLabAsync(db);
        var svc = CreateService(db);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin1!"
        });

        result.Should().NotBeNull();
        result!.User.LabId.Should().Be(labId);
        result.User.LabRole.Should().Be("LabAdmin");
    }

    // ─── REGISTER LAB ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterLab_CreatesLabAndAdminUser()
    {
        using var db = TestFactory.CreateDbContext();
        var svc = CreateService(db);

        var result = await svc.RegisterLabAsync(new RegisterLabRequest
        {
            LabName = "Test Lab",
            InstitutionName = "Test University",
            AdminFullName = "Alice Smith",
            AdminEmail = "alice@testlab.com",
            AdminPassword = "Secure1!"
        });

        result.Should().NotBeNull();
        result!.LabName.Should().Be("Test Lab");
        result.Token.Should().NotBeNullOrEmpty();

        var lab = await db.Labs.FindAsync(result.LabId);
        lab.Should().NotBeNull();
        lab!.LabName.Should().Be("Test Lab");

        var labUsers = db.LabUsers.Where(lu => lu.LabId == result.LabId).ToList();
        labUsers.Should().HaveCount(1);
        labUsers[0].LabRole.Should().Be(LabRole.LabAdmin);
    }

    [Fact]
    public async Task RegisterLab_WithPhone_StoresNormalizedPhone()
    {
        using var db = TestFactory.CreateDbContext();
        var svc = CreateService(db);

        await svc.RegisterLabAsync(new RegisterLabRequest
        {
            LabName = "Phone Lab",
            AdminFullName = "Bob Jones",
            AdminEmail = "bob@phonelab.com",
            AdminPhoneNumber = "+1-800-555-0100",
            AdminPassword = "Secure1!"
        });

        var user = db.Users.FirstOrDefault(u => u.Email == "bob@phonelab.com");
        user.Should().NotBeNull();
        // Should be digits only after normalization
        user!.PhoneNumber.Should().MatchRegex("^[0-9]+$");
    }

    // ─── ADD LAB USER ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddLabUser_ByLabAdmin_CreatesNewMember()
    {
        using var db = TestFactory.CreateDbContext();
        var (adminId, labId) = await SeedUserWithLabAsync(db);
        var svc = CreateService(db);

        var member = await svc.AddLabUserAsync(labId, adminId, new AddLabUserRequest
        {
            FullName = "New Researcher",
            Email = "researcher@test.com",
            Password = "Research1!",
            LabRole = "Researcher"
        });

        member.Should().NotBeNull();
        member.FullName.Should().Be("New Researcher");
        member.LabRole.Should().Be("Researcher");

        var labUser = db.LabUsers.FirstOrDefault(lu => lu.LabId == labId && lu.UserId == member.UserId);
        labUser.Should().NotBeNull();
        labUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AddLabUser_ByNonAdmin_ThrowsUnauthorized()
    {
        using var db = TestFactory.CreateDbContext();
        var (adminId, labId) = await SeedUserWithLabAsync(db);

        // Add a regular researcher
        var researcherId = Guid.NewGuid();
        db.Users.Add(new User { UserId = researcherId, Email = "r@test.com", FullName = "R", IsActive = true, CreatedAt = DateTime.UtcNow, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pw1!") });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = researcherId, LabRole = LabRole.Researcher, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        var act = async () => await svc.AddLabUserAsync(labId, researcherId, new AddLabUserRequest
        {
            FullName = "Another User",
            Email = "another@test.com",
            Password = "Pw1!",
            LabRole = "Researcher"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Lab Admin*");
    }

    // ─── GET LAB MEMBERS ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetLabMembers_ReturnsAllActiveMembers()
    {
        using var db = TestFactory.CreateDbContext();
        var (adminId, labId) = await SeedUserWithLabAsync(db);

        // Add a second member
        var r2Id = Guid.NewGuid();
        db.Users.Add(new User { UserId = r2Id, Email = "r2@test.com", FullName = "R2", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = r2Id, LabRole = LabRole.Researcher, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var members = await svc.GetLabMembersAsync(labId);

        members.Should().HaveCount(2);
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private static async Task SeedUserAsync(GeneFlow.Infrastructure.Data.GeneFlowDbContext db)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "lab@test.com",
            PhoneNumber = "19876543210",
            FullName = "Lab User",
            SystemRole = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private static async Task<(Guid AdminId, Guid LabId)> SeedUserWithLabAsync(GeneFlow.Infrastructure.Data.GeneFlowDbContext db)
    {
        var labId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        db.Labs.Add(new Lab { LabId = labId, LabName = "Alpha Lab", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User
        {
            UserId = adminId,
            Email = "admin@test.com",
            FullName = "Admin User",
            SystemRole = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1!"),
        });
        db.LabUsers.Add(new LabUser
        {
            LabUserId = Guid.NewGuid(),
            LabId = labId,
            UserId = adminId,
            LabRole = LabRole.LabAdmin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        return (adminId, labId);
    }
}
