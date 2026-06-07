using FluentAssertions;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Infrastructure.Services;
using GeneFlow.Tests.Helpers;
using Xunit;

namespace GeneFlow.Tests.Services;

public class PermissionServiceTests
{
    private static (PermissionService svc, GeneFlow.Infrastructure.Data.GeneFlowDbContext db) Create()
    {
        var db = TestFactory.CreateDbContext();
        return (new PermissionService(db), db);
    }

    [Fact]
    public async Task IsLabMember_ForActiveMember_ReturnsTrue()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabMemberAsync(db, LabRole.Researcher);

        var result = await svc.IsLabMemberAsync(userId, labId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLabMember_ForInactiveMember_ReturnsFalse()
    {
        var (svc, db) = Create();
        var labId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Labs.Add(new Lab { LabId = labId, LabName = "Lab", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User { UserId = userId, Email = "x@t.com", FullName = "X", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser
        {
            LabUserId = Guid.NewGuid(),
            LabId = labId,
            UserId = userId,
            LabRole = LabRole.Researcher,
            IsActive = false,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await svc.IsLabMemberAsync(userId, labId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLabAdminOrPI_ForLabAdmin_ReturnsTrue()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabMemberAsync(db, LabRole.LabAdmin);

        var result = await svc.IsLabAdminOrPIAsync(userId, labId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLabAdminOrPI_ForResearcher_ReturnsFalse()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabMemberAsync(db, LabRole.Researcher);

        var result = await svc.IsLabAdminOrPIAsync(userId, labId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanViewExperiment_ByOwner_ReturnsTrue()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabMemberAsync(db, LabRole.Researcher);

        var exp = new Experiment
        {
            ExperimentId = Guid.NewGuid(),
            LabId = labId,
            OwnerUserId = userId,
            ExperimentName = "Test",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            Visibility = ExperimentVisibility.Private,
            Status = ExperimentStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        db.Experiments.Add(exp);
        await db.SaveChangesAsync();

        var result = await svc.CanViewExperimentAsync(userId, exp);
        result.Should().BeTrue(); // Owner always has access
    }

    [Fact]
    public async Task CanViewExperiment_ByLabMember_WithLabVisibility_ReturnsTrue()
    {
        var (svc, db) = Create();
        var (ownerId, labId) = await SeedLabMemberAsync(db, LabRole.LabAdmin);

        var memberId = Guid.NewGuid();
        db.Users.Add(new User { UserId = memberId, Email = "m@t.com", FullName = "Member", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = memberId, LabRole = LabRole.Researcher, IsActive = true, JoinedAt = DateTime.UtcNow });

        var exp = new Experiment
        {
            ExperimentId = Guid.NewGuid(),
            LabId = labId,
            OwnerUserId = ownerId,
            ExperimentName = "Shared",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            Visibility = ExperimentVisibility.Lab,
            Status = ExperimentStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        db.Experiments.Add(exp);
        await db.SaveChangesAsync();

        var result = await svc.CanViewExperimentAsync(memberId, exp);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewExperiment_ByOutsider_ReturnsFalse()
    {
        var (svc, db) = Create();
        var (ownerId, labId) = await SeedLabMemberAsync(db, LabRole.Researcher);

        var exp = new Experiment
        {
            ExperimentId = Guid.NewGuid(),
            LabId = labId,
            OwnerUserId = ownerId,
            ExperimentName = "Private",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            Visibility = ExperimentVisibility.Lab,
            Status = ExperimentStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        db.Experiments.Add(exp);
        await db.SaveChangesAsync();

        var outsiderId = Guid.NewGuid(); // Not a member of this lab
        var result = await svc.CanViewExperimentAsync(outsiderId, exp);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserLabId_ForActiveMember_ReturnsLabId()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabMemberAsync(db, LabRole.Researcher);

        var result = await svc.GetUserLabIdAsync(userId);
        result.Should().Be(labId);
    }

    [Fact]
    public async Task GetUserLabId_ForNonMember_ReturnsNull()
    {
        var (svc, _) = Create();
        var result = await svc.GetUserLabIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private static async Task<(Guid UserId, Guid LabId)> SeedLabMemberAsync(
        GeneFlow.Infrastructure.Data.GeneFlowDbContext db, LabRole role)
    {
        var labId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Labs.Add(new Lab { LabId = labId, LabName = "Lab", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User { UserId = userId, Email = $"{userId}@t.com", FullName = "User", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = userId, LabRole = role, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return (userId, labId);
    }
}
