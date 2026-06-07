using GeneFlow.Core.DTOs.Experiment;

namespace GeneFlow.Core.Interfaces;

public interface IExperimentService
{
    Task<List<ExperimentSummaryDto>> GetExperimentsAsync(Guid userId, Guid labId);
    Task<ExperimentDetailDto?> GetExperimentAsync(Guid experimentId, Guid userId);
    Task<ExperimentDetailDto> CreateExperimentAsync(CreateExperimentRequest request, Guid userId, Guid labId);
    Task<ExperimentDetailDto?> UpdateExperimentAsync(Guid experimentId, UpdateExperimentRequest request, Guid userId);
    Task<bool> DeleteExperimentAsync(Guid experimentId, Guid userId);
    Task<ExperimentDetailDto?> DuplicateExperimentAsync(Guid experimentId, Guid userId);
    Task<bool> FinalizeExperimentAsync(Guid experimentId, Guid userId);
    Task<bool> UnlockExperimentAsync(Guid experimentId, Guid userId);
}
