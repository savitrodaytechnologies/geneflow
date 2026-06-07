namespace GeneFlow.Core.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SystemRole { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LabUser> LabUsers { get; set; } = new List<LabUser>();
    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    public ICollection<Experiment> OwnedExperiments { get; set; } = new List<Experiment>();
    public ICollection<ExperimentNote> Notes { get; set; } = new List<ExperimentNote>();
}
