using GeneFlow.Core.Entities;

namespace GeneFlow.Core.Interfaces;

public interface IPermissionService
{
    Task<bool> CanViewExperimentAsync(Guid userId, Experiment experiment);
    Task<bool> CanEditExperimentAsync(Guid userId, Experiment experiment);
    Task<bool> IsLabAdminOrPIAsync(Guid userId, Guid labId);
    Task<bool> IsLabMemberAsync(Guid userId, Guid labId);
    Task<Guid?> GetUserLabIdAsync(Guid userId);
}
