using GeneFlow.Core.DTOs.Experiment;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Data;
using GeneFlow.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Services;

public class ExperimentService : IExperimentService
{
    private readonly GeneFlowDbContext _db;
    private readonly IPermissionService _permissions;

    public ExperimentService(GeneFlowDbContext db, IPermissionService permissions)
    {
        _db = db;
        _permissions = permissions;
    }

    public async Task<List<ExperimentSummaryDto>> GetExperimentsAsync(Guid userId, Guid labId)
    {
        var experiments = await _db.Experiments
            .AsNoTracking()
            .Include(e => e.Project)
            .Include(e => e.OwnerUser)
            .Where(e => e.LabId == labId)
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .ToListAsync();

        var result = new List<ExperimentSummaryDto>();
        foreach (var e in experiments)
        {
            if (!await _permissions.CanViewExperimentAsync(userId, e)) continue;

            var warningCount = await _db.AnalysisWarnings
                .AsNoTracking()
                .CountAsync(w => w.AnalysisRun.ExperimentId == e.ExperimentId);

            result.Add(new ExperimentSummaryDto
            {
                ExperimentId = e.ExperimentId,
                ExperimentName = e.ExperimentName,
                ProjectName = e.Project?.ProjectName,
                OwnerName = e.OwnerUser.FullName,
                Status = e.Status.ToString(),
                Visibility = e.Visibility.ToString(),
                WarningCount = warningCount,
                LastUpdated = e.UpdatedAt ?? e.CreatedAt,
                ExperimentDate = e.ExperimentDate
            });
        }
        return result;
    }

    public async Task<ExperimentDetailDto?> GetExperimentAsync(Guid experimentId, Guid userId)
    {
        var e = await _db.Experiments
            .AsNoTracking()
            .Include(ex => ex.Project)
            .Include(ex => ex.OwnerUser)
            .FirstOrDefaultAsync(ex => ex.ExperimentId == experimentId);

        if (e is null) return null;
        if (!await _permissions.CanViewExperimentAsync(userId, e)) return null;

        return MapToDetail(e);
    }

    public async Task<ExperimentDetailDto> CreateExperimentAsync(CreateExperimentRequest request, Guid userId, Guid labId)
    {
        var experiment = new Experiment
        {
            ExperimentId = Guid.NewGuid(),
            LabId = labId,
            ProjectId = request.ProjectId,
            OwnerUserId = userId,
            ExperimentName = request.ExperimentName.Trim(),
            ExperimentDate = request.ExperimentDate,
            Objective = request.Objective?.Trim(),
            Hypothesis = request.Hypothesis?.Trim(),
            SampleSource = request.SampleSource?.Trim(),
            TreatmentCondition = request.TreatmentCondition?.Trim(),
            InstrumentName = request.InstrumentName?.Trim(),
            ReferenceGene = request.ReferenceGene?.Trim(),
            ControlSampleName = request.ControlSampleName?.Trim(),
            PlateType = "96-well",
            Status = ExperimentStatus.Draft,
            Visibility = request.Visibility,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Experiments.Add(experiment);

        // Auto-create one 96-well plate layout and 96 wells
        var layout = new PlateLayout
        {
            PlateLayoutId = Guid.NewGuid(),
            ExperimentId = experiment.ExperimentId,
            PlateType = "96-well",
            RowCount = 8,
            ColumnCount = 12,
            CreatedAt = DateTime.UtcNow
        };
        _db.PlateLayouts.Add(layout);

        var wellIds = WellIdHelper.Generate96WellIds();
        foreach (var wellId in wellIds)
        {
            _db.PlateWells.Add(new PlateWell
            {
                PlateWellId = Guid.NewGuid(),
                PlateLayoutId = layout.PlateLayoutId,
                ExperimentId = experiment.ExperimentId,
                WellId = wellId,
                RowLabel = wellId[0].ToString(),
                ColumnNumber = int.Parse(wellId[1..]),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return MapToDetail(experiment);
    }

    public async Task<ExperimentDetailDto?> UpdateExperimentAsync(Guid experimentId, UpdateExperimentRequest request, Guid userId)
    {
        var experiment = await _db.Experiments.FindAsync(experimentId);
        if (experiment is null) return null;
        if (!await _permissions.CanEditExperimentAsync(userId, experiment)) return null;

        experiment.ExperimentName = request.ExperimentName.Trim();
        experiment.ExperimentDate = request.ExperimentDate;
        experiment.Objective = request.Objective?.Trim();
        experiment.Hypothesis = request.Hypothesis?.Trim();
        experiment.SampleSource = request.SampleSource?.Trim();
        experiment.TreatmentCondition = request.TreatmentCondition?.Trim();
        experiment.InstrumentName = request.InstrumentName?.Trim();
        experiment.ReferenceGene = request.ReferenceGene?.Trim();
        experiment.ControlSampleName = request.ControlSampleName?.Trim();
        experiment.Visibility = request.Visibility;
        experiment.Notes = request.Notes?.Trim();
        experiment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetExperimentAsync(experimentId, userId);
    }

    public async Task<bool> DeleteExperimentAsync(Guid experimentId, Guid userId)
    {
        var experiment = await _db.Experiments.FindAsync(experimentId);
        if (experiment is null) return false;
        if (!await _permissions.CanEditExperimentAsync(userId, experiment)) return false;

        experiment.IsDeleted = true;
        experiment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ExperimentDetailDto?> DuplicateExperimentAsync(Guid experimentId, Guid userId)
    {
        var source = await _db.Experiments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExperimentId == experimentId);

        if (source is null) return null;
        if (!await _permissions.CanViewExperimentAsync(userId, source)) return null;

        var createRequest = new CreateExperimentRequest
        {
            ProjectId = source.ProjectId,
            ExperimentName = $"{source.ExperimentName} (Copy)",
            ExperimentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Objective = source.Objective,
            Hypothesis = source.Hypothesis,
            SampleSource = source.SampleSource,
            TreatmentCondition = source.TreatmentCondition,
            InstrumentName = source.InstrumentName,
            ReferenceGene = source.ReferenceGene,
            ControlSampleName = source.ControlSampleName,
            Visibility = source.Visibility,
        };

        var newExperiment = await CreateExperimentAsync(createRequest, userId, source.LabId);

        // Copy plate well layout (sample names, targets etc — not Ct values)
        var sourceWells = await _db.PlateWells
            .AsNoTracking()
            .Where(w => w.ExperimentId == experimentId)
            .ToListAsync();

        var newLayout = await _db.PlateLayouts
            .FirstAsync(pl => pl.ExperimentId == newExperiment.ExperimentId);

        var newWells = await _db.PlateWells
            .Where(w => w.ExperimentId == newExperiment.ExperimentId)
            .ToListAsync();

        foreach (var newWell in newWells)
        {
            var sourceWell = sourceWells.FirstOrDefault(sw => sw.WellId == newWell.WellId);
            if (sourceWell is null) continue;

            newWell.SampleName = sourceWell.SampleName;
            newWell.TargetGene = sourceWell.TargetGene;
            newWell.ReferenceGene = sourceWell.ReferenceGene;
            newWell.SampleType = sourceWell.SampleType;
            newWell.ReplicateGroup = sourceWell.ReplicateGroup;
            newWell.UpdatedAt = DateTime.UtcNow;
            // Do NOT copy: CtValue, IsExcluded, ExclusionReason
        }

        await _db.SaveChangesAsync();
        return newExperiment;
    }

    public async Task<bool> FinalizeExperimentAsync(Guid experimentId, Guid userId)
    {
        var experiment = await _db.Experiments.FindAsync(experimentId);
        if (experiment is null) return false;
        if (!await _permissions.CanEditExperimentAsync(userId, experiment)) return false;

        experiment.Status = ExperimentStatus.Finalized;
        experiment.LockedAt = DateTime.UtcNow;
        experiment.LockedByUserId = userId;
        experiment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlockExperimentAsync(Guid experimentId, Guid userId)
    {
        var experiment = await _db.Experiments.FindAsync(experimentId);
        if (experiment is null) return false;

        // Only lab admin/PI can unlock
        if (!await _permissions.IsLabAdminOrPIAsync(userId, experiment.LabId)) return false;

        experiment.LockedAt = null;
        experiment.LockedByUserId = null;
        experiment.Status = ExperimentStatus.Analyzed;
        experiment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static ExperimentDetailDto MapToDetail(Experiment e) => new()
    {
        ExperimentId = e.ExperimentId,
        LabId = e.LabId,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.ProjectName,
        OwnerUserId = e.OwnerUserId,
        OwnerName = e.OwnerUser?.FullName ?? string.Empty,
        ExperimentName = e.ExperimentName,
        ExperimentDate = e.ExperimentDate,
        Objective = e.Objective,
        Hypothesis = e.Hypothesis,
        SampleSource = e.SampleSource,
        TreatmentCondition = e.TreatmentCondition,
        InstrumentName = e.InstrumentName,
        ReferenceGene = e.ReferenceGene,
        ControlSampleName = e.ControlSampleName,
        PlateType = e.PlateType,
        Status = e.Status.ToString(),
        Visibility = e.Visibility.ToString(),
        Notes = e.Notes,
        IsLocked = e.LockedAt.HasValue,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
