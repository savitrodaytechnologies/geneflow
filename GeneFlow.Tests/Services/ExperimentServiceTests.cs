using FluentAssertions;
using GeneFlow.Core.DTOs.Experiment;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Infrastructure.Services;
using GeneFlow.Tests.Helpers;
using Xunit;

namespace GeneFlow.Tests.Services;

public class ExperimentServiceTests
{
    private static (ExperimentService svc, GeneFlow.Infrastructure.Data.GeneFlowDbContext db) Create()
    {
        var db = TestFactory.CreateDbContext();
        var permissions = new PermissionService(db);
        return (new ExperimentService(db, permissions), db);
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateExperiment_SetsCorrectDefaults()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);

        var result = await svc.CreateExperimentAsync(new CreateExperimentRequest
        {
            ExperimentName = "CCR2_Run1",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            ReferenceGene = "GAPDH",
            ControlSampleName = "Control",
            Visibility = ExperimentVisibility.Lab,
        }, userId, labId);

        result.ExperimentName.Should().Be("CCR2_Run1");
        result.Status.Should().Be("Draft");
        result.OwnerName.Should().NotBeNullOrEmpty();

        // Should have auto-created 96 plate wells
        var wells = db.PlateWells.Where(w => w.ExperimentId == result.ExperimentId).ToList();
        wells.Should().HaveCount(96);
    }

    [Fact]
    public async Task CreateExperiment_TrimsExperimentName()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);

        var result = await svc.CreateExperimentAsync(new CreateExperimentRequest
        {
            ExperimentName = "  Trimmed Name  ",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            ReferenceGene = "ACTB",
            ControlSampleName = "Ctrl",
        }, userId, labId);

        result.ExperimentName.Should().Be("Trimmed Name");
    }

    // ─── GET ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetExperiment_ByOwner_ReturnsExperiment()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);
        var created = await svc.CreateExperimentAsync(BasicRequest(), userId, labId);

        var fetched = await svc.GetExperimentAsync(created.ExperimentId, userId);

        fetched.Should().NotBeNull();
        fetched!.ExperimentId.Should().Be(created.ExperimentId);
    }

    [Fact]
    public async Task GetExperiment_ByLabMember_ReturnsExperiment_WhenLabVisibility()
    {
        var (svc, db) = Create();
        var (ownerId, labId) = await SeedLabAsync(db);

        // Add second lab member
        var memberId = Guid.NewGuid();
        db.Users.Add(new User { UserId = memberId, Email = "m@t.com", FullName = "Member", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = memberId, LabRole = LabRole.Researcher, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var created = await svc.CreateExperimentAsync(new CreateExperimentRequest
        {
            ExperimentName = "LabVisible",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            ReferenceGene = "GAPDH",
            ControlSampleName = "Ctrl",
            Visibility = ExperimentVisibility.Lab,
        }, ownerId, labId);

        var fetched = await svc.GetExperimentAsync(created.ExperimentId, memberId);
        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExperiment_ByOutsider_ReturnsNull()
    {
        var (svc, db) = Create();
        var (ownerId, labId) = await SeedLabAsync(db);
        var created = await svc.CreateExperimentAsync(BasicRequest(), ownerId, labId);

        var outsiderId = Guid.NewGuid();
        var fetched = await svc.GetExperimentAsync(created.ExperimentId, outsiderId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task GetExperiment_WithInvalidId_ReturnsNull()
    {
        var (svc, _) = Create();
        var fetched = await svc.GetExperimentAsync(Guid.NewGuid(), Guid.NewGuid());
        fetched.Should().BeNull();
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateExperiment_ByOwner_UpdatesFields()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);
        var created = await svc.CreateExperimentAsync(BasicRequest(), userId, labId);

        var updated = await svc.UpdateExperimentAsync(created.ExperimentId, new UpdateExperimentRequest
        {
            ExperimentName = "Updated Name",
            ReferenceGene = "ACTB",
            ControlSampleName = "Mock",
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
        }, userId);

        updated.Should().NotBeNull();
        updated!.ExperimentName.Should().Be("Updated Name");
        updated.ReferenceGene.Should().Be("ACTB");
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteExperiment_ByOwner_SoftDeletes()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);
        var created = await svc.CreateExperimentAsync(BasicRequest(), userId, labId);

        var deleted = await svc.DeleteExperimentAsync(created.ExperimentId, userId);
        deleted.Should().BeTrue();

        // Soft delete — record still in DB but not fetchable via service
        var fetched = await svc.GetExperimentAsync(created.ExperimentId, userId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteExperiment_WithFakeId_ReturnsFalse()
    {
        var (svc, _) = Create();
        var result = await svc.DeleteExperimentAsync(Guid.NewGuid(), Guid.NewGuid());
        result.Should().BeFalse();
    }

    // ─── DUPLICATE ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DuplicateExperiment_CreatesCopyWithCopyPrefix()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);
        var original = await svc.CreateExperimentAsync(BasicRequest(), userId, labId);

        var duplicate = await svc.DuplicateExperimentAsync(original.ExperimentId, userId);

        duplicate.Should().NotBeNull();
        duplicate!.ExperimentId.Should().NotBe(original.ExperimentId);
        duplicate.ExperimentName.Should().Contain("Copy");
    }

    // ─── LIST ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetExperiments_ReturnsOnlyLabExperiments()
    {
        var (svc, db) = Create();
        var (userId, labId) = await SeedLabAsync(db);

        await svc.CreateExperimentAsync(BasicRequest("E1"), userId, labId);
        await svc.CreateExperimentAsync(BasicRequest("E2"), userId, labId);

        // Create a different lab and experiment — should not appear
        var otherLabId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        db.Labs.Add(new Lab { LabId = otherLabId, LabName = "Other Lab", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User { UserId = otherUserId, Email = "other@t.com", FullName = "Other", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = otherLabId, UserId = otherUserId, LabRole = LabRole.LabAdmin, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        await svc.CreateExperimentAsync(BasicRequest("OtherE"), otherUserId, otherLabId);

        var myExperiments = await svc.GetExperimentsAsync(userId, labId);
        myExperiments.Should().HaveCount(2);
        myExperiments.Should().OnlyContain(e => e.ExperimentName.StartsWith("E"));
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private static CreateExperimentRequest BasicRequest(string name = "TestExp") =>
        new CreateExperimentRequest
        {
            ExperimentName = name,
            ExperimentDate = DateOnly.FromDateTime(DateTime.Today),
            ReferenceGene = "GAPDH",
            ControlSampleName = "Control",
            Visibility = ExperimentVisibility.Lab,
        };

    private static async Task<(Guid UserId, Guid LabId)> SeedLabAsync(GeneFlow.Infrastructure.Data.GeneFlowDbContext db)
    {
        var labId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Labs.Add(new Lab { LabId = labId, LabName = "Test Lab", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User { UserId = userId, Email = "owner@test.com", FullName = "Lab Owner", SystemRole = "User", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.LabUsers.Add(new LabUser { LabUserId = Guid.NewGuid(), LabId = labId, UserId = userId, LabRole = LabRole.LabAdmin, IsActive = true, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return (userId, labId);
    }
}
