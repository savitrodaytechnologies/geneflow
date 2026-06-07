using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly GeneFlowDbContext _db;

    public PermissionService(GeneFlowDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> GetUserLabIdAsync(Guid userId)
    {
        var labUser = await _db.LabUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.UserId == userId && lu.IsActive);
        return labUser?.LabId;
    }

    public async Task<bool> IsLabMemberAsync(Guid userId, Guid labId)
    {
        return await _db.LabUsers
            .AnyAsync(lu => lu.UserId == userId && lu.LabId == labId && lu.IsActive);
    }

    public async Task<bool> IsLabAdminOrPIAsync(Guid userId, Guid labId)
    {
        return await _db.LabUsers
            .AnyAsync(lu => lu.UserId == userId && lu.LabId == labId && lu.IsActive
                && (lu.LabRole == LabRole.LabAdmin || lu.LabRole == LabRole.PI));
    }

    public async Task<bool> CanViewExperimentAsync(Guid userId, Experiment experiment)
    {
        // Owner can always view
        if (experiment.OwnerUserId == userId) return true;

        // Check if user is lab admin/PI
        if (await IsLabAdminOrPIAsync(userId, experiment.LabId)) return true;

        // Lab visibility: any lab member
        if (experiment.Visibility == ExperimentVisibility.Lab)
            return await IsLabMemberAsync(userId, experiment.LabId);

        // Project visibility: project member
        if (experiment.Visibility == ExperimentVisibility.Project && experiment.ProjectId.HasValue)
            return await _db.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == experiment.ProjectId && pu.IsActive);

        return false;
    }

    public async Task<bool> CanEditExperimentAsync(Guid userId, Experiment experiment)
    {
        // Finalized experiments cannot be edited by anyone through normal flow
        if (experiment.LockedAt.HasValue) return false;

        // Owner can edit
        if (experiment.OwnerUserId == userId) return true;

        // Lab admin/PI can edit
        return await IsLabAdminOrPIAsync(userId, experiment.LabId);
    }
}
