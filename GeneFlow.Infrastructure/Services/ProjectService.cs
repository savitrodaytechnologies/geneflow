using GeneFlow.Core.DTOs.Project;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly GeneFlowDbContext _db;

    public ProjectService(GeneFlowDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectDto>> GetProjectsAsync(Guid userId, Guid labId)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.LabId == labId)
            .Select(p => new ProjectDto
            {
                ProjectId = p.ProjectId,
                LabId = p.LabId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                CreatedByUserName = p.CreatedByUser.FullName,
                ExperimentCount = p.Experiments.Count(e => !e.IsDeleted),
                CreatedAt = p.CreatedAt
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectAsync(Guid projectId, Guid userId)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .Select(p => new ProjectDto
            {
                ProjectId = p.ProjectId,
                LabId = p.LabId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                CreatedByUserName = p.CreatedByUser.FullName,
                ExperimentCount = p.Experiments.Count(e => !e.IsDeleted),
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid userId, Guid labId)
    {
        var project = new Project
        {
            ProjectId = Guid.NewGuid(),
            LabId = labId,
            ProjectName = request.ProjectName.Trim(),
            Description = request.Description?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);

        // Add creator as project member
        _db.ProjectUsers.Add(new ProjectUser
        {
            ProjectUserId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            UserId = userId,
            ProjectRole = "Lead",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new ProjectDto
        {
            ProjectId = project.ProjectId,
            LabId = project.LabId,
            ProjectName = project.ProjectName,
            Description = project.Description,
            CreatedByUserName = string.Empty,
            ExperimentCount = 0,
            CreatedAt = project.CreatedAt
        };
    }

    public async Task<ProjectDto?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, Guid userId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return null;

        project.ProjectName = request.ProjectName.Trim();
        project.Description = request.Description?.Trim();
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetProjectAsync(projectId, userId);
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return false;

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
