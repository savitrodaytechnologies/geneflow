using GeneFlow.Core.DTOs.Project;

namespace GeneFlow.Core.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetProjectsAsync(Guid userId, Guid labId);
    Task<ProjectDto?> GetProjectAsync(Guid projectId, Guid userId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid userId, Guid labId);
    Task<ProjectDto?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, Guid userId);
    Task<bool> DeleteProjectAsync(Guid projectId, Guid userId);
}
