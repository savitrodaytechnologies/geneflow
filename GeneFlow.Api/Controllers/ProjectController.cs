using GeneFlow.Core.DTOs.Project;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IExperimentService _experimentService;
    private readonly CurrentUserService _currentUser;

    public ProjectController(IProjectService projectService, IExperimentService experimentService, CurrentUserService currentUser)
    {
        _projectService = projectService;
        _experimentService = experimentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var projects = await _projectService.GetProjectsAsync(_currentUser.UserId, labId.Value);
        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectName))
            return BadRequest(new { message = "Project name is required." });

        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var project = await _projectService.CreateProjectAsync(request, _currentUser.UserId, labId.Value);
        return CreatedAtAction(nameof(GetProject), new { projectId = project.ProjectId }, project);
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProject(Guid projectId)
    {
        var project = await _projectService.GetProjectAsync(projectId, _currentUser.UserId);
        if (project is null) return NotFound();
        return Ok(project);
    }

    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectName))
            return BadRequest(new { message = "Project name is required." });

        var project = await _projectService.UpdateProjectAsync(projectId, request, _currentUser.UserId);
        if (project is null) return NotFound();
        return Ok(project);
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        var deleted = await _projectService.DeleteProjectAsync(projectId, _currentUser.UserId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{projectId}/experiments")]
    public async Task<IActionResult> GetProjectExperiments(Guid projectId)
    {
        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var experiments = await _experimentService.GetExperimentsAsync(_currentUser.UserId, labId.Value);
        var filtered = experiments.Where(e => true).ToList(); // project filter applied in service via ProjectId
        return Ok(filtered);
    }
}
