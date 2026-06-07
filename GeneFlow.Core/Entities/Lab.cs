namespace GeneFlow.Core.Entities;

public class Lab
{
    public Guid LabId { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? InstitutionName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LabUser> LabUsers { get; set; } = new List<LabUser>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();
}
