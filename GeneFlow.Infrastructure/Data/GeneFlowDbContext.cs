using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Data;

public class GeneFlowDbContext : DbContext
{
    public GeneFlowDbContext(DbContextOptions<GeneFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Lab> Labs => Set<Lab>();
    public DbSet<LabUser> LabUsers => Set<LabUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<Experiment> Experiments => Set<Experiment>();
    public DbSet<PlateLayout> PlateLayouts => Set<PlateLayout>();
    public DbSet<PlateWell> PlateWells => Set<PlateWell>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    public DbSet<RawCtData> RawCtData => Set<RawCtData>();
    public DbSet<AnalysisRun> AnalysisRuns => Set<AnalysisRun>();
    public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();
    public DbSet<AnalysisWarning> AnalysisWarnings => Set<AnalysisWarning>();
    public DbSet<ExperimentNote> ExperimentNotes => Set<ExperimentNote>();
    public DbSet<ExportFile> ExportFiles => Set<ExportFile>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Users ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.Property(u => u.UserId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.Property(u => u.SystemRole).HasMaxLength(50).HasDefaultValue("User");
            e.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ── Labs ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Lab>(e =>
        {
            e.HasKey(l => l.LabId);
            e.Property(l => l.LabId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(l => l.LabName).HasMaxLength(200).IsRequired();
            e.Property(l => l.InstitutionName).HasMaxLength(200);
            e.Property(l => l.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ── LabUsers ───────────────────────────────────────────────────────
        modelBuilder.Entity<LabUser>(e =>
        {
            e.HasKey(lu => lu.LabUserId);
            e.Property(lu => lu.LabUserId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(lu => lu.LabRole).HasConversion<string>().HasMaxLength(50);
            e.Property(lu => lu.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(lu => new { lu.LabId, lu.UserId }).IsUnique();
            e.HasOne(lu => lu.Lab).WithMany(l => l.LabUsers).HasForeignKey(lu => lu.LabId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(lu => lu.User).WithMany(u => u.LabUsers).HasForeignKey(lu => lu.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Projects ───────────────────────────────────────────────────────
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.ProjectId);
            e.Property(p => p.ProjectId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(p => p.ProjectName).HasMaxLength(200).IsRequired();
            e.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasQueryFilter(p => !p.IsDeleted);
            e.HasOne(p => p.Lab).WithMany(l => l.Projects).HasForeignKey(p => p.LabId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.CreatedByUser).WithMany().HasForeignKey(p => p.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ProjectUsers ───────────────────────────────────────────────────
        modelBuilder.Entity<ProjectUser>(e =>
        {
            e.HasKey(pu => pu.ProjectUserId);
            e.Property(pu => pu.ProjectUserId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(pu => pu.ProjectRole).HasMaxLength(50).HasDefaultValue("Member");
            e.Property(pu => pu.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(pu => new { pu.ProjectId, pu.UserId }).IsUnique();
            e.HasOne(pu => pu.Project).WithMany(p => p.ProjectUsers).HasForeignKey(pu => pu.ProjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pu => pu.User).WithMany(u => u.ProjectUsers).HasForeignKey(pu => pu.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Experiments ────────────────────────────────────────────────────
        modelBuilder.Entity<Experiment>(e =>
        {
            e.HasKey(ex => ex.ExperimentId);
            e.Property(ex => ex.ExperimentId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(ex => ex.ExperimentName).HasMaxLength(200).IsRequired();
            e.Property(ex => ex.PlateType).HasMaxLength(50).HasDefaultValue("96-well");
            e.Property(ex => ex.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(ex => ex.Visibility).HasConversion<string>().HasMaxLength(50);
            e.Property(ex => ex.SampleSource).HasMaxLength(300);
            e.Property(ex => ex.TreatmentCondition).HasMaxLength(300);
            e.Property(ex => ex.InstrumentName).HasMaxLength(200);
            e.Property(ex => ex.ReferenceGene).HasMaxLength(100);
            e.Property(ex => ex.ControlSampleName).HasMaxLength(200);
            e.Property(ex => ex.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasQueryFilter(ex => !ex.IsDeleted);
            e.HasOne(ex => ex.Lab).WithMany(l => l.Experiments).HasForeignKey(ex => ex.LabId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ex => ex.Project).WithMany(p => p.Experiments).HasForeignKey(ex => ex.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ex => ex.OwnerUser).WithMany(u => u.OwnedExperiments).HasForeignKey(ex => ex.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── PlateLayouts ───────────────────────────────────────────────────
        modelBuilder.Entity<PlateLayout>(e =>
        {
            e.HasKey(pl => pl.PlateLayoutId);
            e.Property(pl => pl.PlateLayoutId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(pl => pl.PlateType).HasMaxLength(50).HasDefaultValue("96-well");
            e.Property(pl => pl.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(pl => pl.Experiment).WithMany(ex => ex.PlateLayouts).HasForeignKey(pl => pl.ExperimentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── PlateWells ─────────────────────────────────────────────────────
        modelBuilder.Entity<PlateWell>(e =>
        {
            e.HasKey(pw => pw.PlateWellId);
            e.Property(pw => pw.PlateWellId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(pw => pw.WellId).HasMaxLength(5).IsRequired();
            e.Property(pw => pw.RowLabel).HasMaxLength(2).IsRequired();
            e.Property(pw => pw.SampleName).HasMaxLength(200);
            e.Property(pw => pw.TargetGene).HasMaxLength(100);
            e.Property(pw => pw.ReferenceGene).HasMaxLength(100);
            e.Property(pw => pw.SampleType).HasMaxLength(50);
            e.Property(pw => pw.ReplicateGroup).HasMaxLength(100);
            e.Property(pw => pw.ExclusionReason).HasMaxLength(300);
            e.Property(pw => pw.CtValue).HasColumnType("decimal(10,4)");
            e.Property(pw => pw.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(pw => new { pw.PlateLayoutId, pw.WellId }).IsUnique();
            e.HasOne(pw => pw.PlateLayout).WithMany(pl => pl.PlateWells).HasForeignKey(pw => pw.PlateLayoutId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pw => pw.Experiment).WithMany(ex => ex.PlateWells).HasForeignKey(pw => pw.ExperimentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── UploadedFiles ──────────────────────────────────────────────────
        modelBuilder.Entity<UploadedFile>(e =>
        {
            e.HasKey(uf => uf.UploadedFileId);
            e.Property(uf => uf.UploadedFileId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(uf => uf.OriginalFileName).HasMaxLength(300).IsRequired();
            e.Property(uf => uf.StoredFilePath).HasMaxLength(1000).IsRequired();
            e.Property(uf => uf.FileType).HasMaxLength(50).IsRequired();
            e.Property(uf => uf.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(uf => uf.Experiment).WithMany(ex => ex.UploadedFiles).HasForeignKey(uf => uf.ExperimentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(uf => uf.UploadedByUser).WithMany().HasForeignKey(uf => uf.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── RawCtData ──────────────────────────────────────────────────────
        modelBuilder.Entity<RawCtData>(e =>
        {
            e.HasKey(r => r.RawCtDataId);
            e.Property(r => r.RawCtDataId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(r => r.WellId).HasMaxLength(5).IsRequired();
            e.Property(r => r.SampleNameFromFile).HasMaxLength(200);
            e.Property(r => r.TargetGeneFromFile).HasMaxLength(100);
            e.Property(r => r.CtValueRaw).HasMaxLength(100);
            e.Property(r => r.CtValue).HasColumnType("decimal(10,4)");
            e.Property(r => r.ValidationMessage).HasMaxLength(500);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(r => r.Experiment).WithMany().HasForeignKey(r => r.ExperimentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.UploadedFile).WithMany(uf => uf.RawCtData).HasForeignKey(r => r.UploadedFileId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AnalysisRuns ───────────────────────────────────────────────────
        modelBuilder.Entity<AnalysisRun>(e =>
        {
            e.HasKey(ar => ar.AnalysisRunId);
            e.Property(ar => ar.AnalysisRunId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(ar => ar.RunName).HasMaxLength(200);
            e.Property(ar => ar.ControlSampleName).HasMaxLength(200).IsRequired();
            e.Property(ar => ar.ReferenceGene).HasMaxLength(100).IsRequired();
            e.Property(ar => ar.CalculationMethod).HasMaxLength(100).HasDefaultValue("DeltaDeltaCt");
            e.Property(ar => ar.HighCtThreshold).HasColumnType("decimal(10,4)").HasDefaultValue(35m);
            e.Property(ar => ar.ReplicateSdThreshold).HasColumnType("decimal(10,4)").HasDefaultValue(0.5m);
            e.Property(ar => ar.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(ar => ar.Experiment).WithMany(ex => ex.AnalysisRuns).HasForeignKey(ar => ar.ExperimentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ar => ar.CreatedByUser).WithMany().HasForeignKey(ar => ar.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AnalysisResults ────────────────────────────────────────────────
        modelBuilder.Entity<AnalysisResult>(e =>
        {
            e.HasKey(ar => ar.AnalysisResultId);
            e.Property(ar => ar.AnalysisResultId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(ar => ar.SampleName).HasMaxLength(200).IsRequired();
            e.Property(ar => ar.TargetGene).HasMaxLength(100).IsRequired();
            e.Property(ar => ar.ReferenceGene).HasMaxLength(100).IsRequired();
            e.Property(ar => ar.SampleType).HasMaxLength(50);
            e.Property(ar => ar.MeanTargetCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.SdTargetCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.MeanReferenceCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.SdReferenceCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.DeltaCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.DeltaDeltaCt).HasColumnType("decimal(10,4)");
            e.Property(ar => ar.FoldChange).HasColumnType("decimal(18,8)");
            e.Property(ar => ar.Log2FoldChange).HasColumnType("decimal(18,8)");
            e.Property(ar => ar.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(ar => ar.AnalysisRun).WithMany(r => r.AnalysisResults).HasForeignKey(ar => ar.AnalysisRunId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ar => ar.Experiment).WithMany().HasForeignKey(ar => ar.ExperimentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AnalysisWarnings ───────────────────────────────────────────────
        modelBuilder.Entity<AnalysisWarning>(e =>
        {
            e.HasKey(aw => aw.AnalysisWarningId);
            e.Property(aw => aw.AnalysisWarningId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(aw => aw.WarningType).HasMaxLength(100).IsRequired();
            e.Property(aw => aw.WarningMessage).HasMaxLength(500).IsRequired();
            e.Property(aw => aw.Severity).HasConversion<string>().HasMaxLength(50);
            e.Property(aw => aw.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(aw => aw.AnalysisRun).WithMany(r => r.AnalysisWarnings).HasForeignKey(aw => aw.AnalysisRunId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(aw => aw.AnalysisResult).WithMany(r => r.AnalysisWarnings).HasForeignKey(aw => aw.AnalysisResultId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExperimentNotes ────────────────────────────────────────────────
        modelBuilder.Entity<ExperimentNote>(e =>
        {
            e.HasKey(n => n.ExperimentNoteId);
            e.Property(n => n.ExperimentNoteId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(n => n.NoteText).IsRequired();
            e.Property(n => n.NoteType).HasConversion<string>().HasMaxLength(50);
            e.Property(n => n.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(n => n.Experiment).WithMany(ex => ex.ExperimentNotes).HasForeignKey(n => n.ExperimentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.User).WithMany(u => u.Notes).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExportFiles ────────────────────────────────────────────────────
        modelBuilder.Entity<ExportFile>(e =>
        {
            e.HasKey(ef => ef.ExportFileId);
            e.Property(ef => ef.ExportFileId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(ef => ef.ExportType).HasMaxLength(50).IsRequired();
            e.Property(ef => ef.FileName).HasMaxLength(300).IsRequired();
            e.Property(ef => ef.FilePath).HasMaxLength(1000).IsRequired();
            e.Property(ef => ef.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(ef => ef.Experiment).WithMany().HasForeignKey(ef => ef.ExperimentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ef => ef.AnalysisRun).WithMany().HasForeignKey(ef => ef.AnalysisRunId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ef => ef.CreatedByUser).WithMany().HasForeignKey(ef => ef.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AuditLogs ──────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(al => al.AuditLogId);
            e.Property(al => al.AuditLogId).HasDefaultValueSql("NEWSEQUENTIALID()");
            e.Property(al => al.EntityName).HasMaxLength(100).IsRequired();
            e.Property(al => al.Action).HasMaxLength(100).IsRequired();
            e.Property(al => al.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            // No FK constraints on AuditLogs — logs must survive entity deletion
        });
    }
}
