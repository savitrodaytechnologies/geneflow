namespace GeneFlow.Core.DTOs.Project;

public class ProjectDto
{
    public Guid ProjectId { get; set; }
    public Guid LabId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int ExperimentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProjectRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
