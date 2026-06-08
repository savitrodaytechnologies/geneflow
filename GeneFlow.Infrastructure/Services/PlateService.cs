using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GeneFlow.Core.DTOs.Plate;
using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Data;
using GeneFlow.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Infrastructure.Services
{
    public class PlateService : IPlateService
    {
        private readonly GeneFlowDbContext _db;
        private readonly IPermissionService _permissions;

        public PlateService(GeneFlowDbContext db, IPermissionService permissions)
        {
            _db = db;
            _permissions = permissions;
        }

        public async Task<PlateGridDto?> GetPlateGridAsync(Guid experimentId, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return null;
            if (!await _permissions.CanViewExperimentAsync(currentUserId, experiment)) return null;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return null;

            var dto = new PlateGridDto
            {
                PlateLayoutId = layout.PlateLayoutId,
                ExperimentId = experimentId,
                LayoutName = $"{layout.PlateType} Layout",
                TotalWells = layout.PlateWells.Count,
                FilledWells = layout.PlateWells.Count(w => !string.IsNullOrEmpty(w.SampleName)),
                ExcludedWells = layout.PlateWells.Count(w => w.IsExcluded),
                Grid = new List<List<PlateWellDto>>()
            };

            var wellLookup = layout.PlateWells.ToDictionary(w => w.WellId);
            string[] rows = { "A", "B", "C", "D", "E", "F", "G", "H" };

            for (int r = 0; r < 8; r++)
            {
                var rowList = new List<PlateWellDto>();
                for (int c = 1; c <= 12; c++)
                {
                    string wellId = $"{rows[r]}{c:D2}";
                    if (wellLookup.TryGetValue(wellId, out var well))
                    {
                        rowList.Add(MapToDto(well, r, c - 1));
                    }
                }
                dto.Grid.Add(rowList);
            }

            return dto;
        }

        public async Task<List<PlateWellDto>> GetPlateListAsync(Guid experimentId, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return new List<PlateWellDto>();
            if (!await _permissions.CanViewExperimentAsync(currentUserId, experiment)) return new List<PlateWellDto>();

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return new List<PlateWellDto>();

            string[] rows = { "A", "B", "C", "D", "E", "F", "G", "H" };

            return layout.PlateWells
                .OrderBy(w => w.WellId)
                .Select(w =>
                {
                    int rowIdx = w.WellId.Length > 0 ? Array.IndexOf(rows, w.WellId[0].ToString()) : 0;
                    int colIdx = w.WellId.Length > 1 && int.TryParse(w.WellId[1..], out var c) ? c - 1 : 0;
                    return MapToDto(w, rowIdx, colIdx);
                })
                .ToList();
        }

        public async Task<PlateWellDto?> UpdateWellAsync(Guid experimentId, string wellId, UpdateWellRequest request, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return null;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return null;

            var normalizedWellId = WellIdHelper.NormalizeWellId(wellId);
            if (normalizedWellId == null) return null;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return null;

            var well = layout.PlateWells.FirstOrDefault(w => w.WellId == normalizedWellId);
            if (well == null) return null;

            ApplyWellUpdate(well, request);
            well.UpdatedAt = DateTime.UtcNow;

            MarkPlateDesignedIfNeeded(experiment);
            await _db.SaveChangesAsync();

            string[] rows = { "A", "B", "C", "D", "E", "F", "G", "H" };
            int rowIdx = Array.IndexOf(rows, well.WellId[0].ToString());
            int colIdx = int.TryParse(well.WellId[1..], out var col) ? col - 1 : 0;
            return MapToDto(well, rowIdx, colIdx);
        }

        public async Task<int> BulkUpdateWellsAsync(Guid experimentId, BulkUpdateWellsRequest request, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return 0;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return 0;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return 0;

            int updated = 0;
            foreach (var item in request.Wells)
            {
                var normalized = WellIdHelper.NormalizeWellId(item.WellId);
                if (normalized == null) continue;

                var well = layout.PlateWells.FirstOrDefault(w => w.WellId == normalized);
                if (well == null) continue;

                ApplyWellUpdate(well, new UpdateWellRequest
                {
                    SampleName = item.SampleName,
                    TargetGene = item.TargetGene,
                    ReferenceGene = item.ReferenceGene,
                    SampleType = item.SampleType,
                    ReplicateGroup = item.ReplicateGroup
                });
                well.UpdatedAt = DateTime.UtcNow;
                updated++;
            }

            if (updated > 0)
            {
                MarkPlateDesignedIfNeeded(experiment);
                await _db.SaveChangesAsync();
            }

            return updated;
        }

        public async Task<int> QuickFillAsync(Guid experimentId, QuickFillRequest request, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return 0;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return 0;

            var wellIds = request.FillByColumn
                ? WellIdHelper.GenerateWellRangeByColumn(request.FromWell, request.ToWell)
                : WellIdHelper.GenerateWellRange(request.FromWell, request.ToWell);
            if (wellIds.Count == 0) return 0;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return 0;

            var updateReq = new UpdateWellRequest
            {
                SampleName = request.SampleName,
                TargetGene = request.TargetGene,
                ReferenceGene = request.ReferenceGene,
                SampleType = request.SampleType,
                ReplicateGroup = request.ReplicateGroup
            };

            int updated = 0;
            foreach (var wid in wellIds)
            {
                var well = layout.PlateWells.FirstOrDefault(w => w.WellId == wid);
                if (well == null) continue;
                ApplyWellUpdate(well, updateReq);
                well.UpdatedAt = DateTime.UtcNow;
                updated++;
            }

            if (updated > 0)
            {
                MarkPlateDesignedIfNeeded(experiment);
                await _db.SaveChangesAsync();
            }

            return updated;
        }

        public async Task<UploadLayoutCsvResult> UploadLayoutCsvAsync(Guid experimentId, Stream csvStream, string fileName, Guid currentUserId)
        {
            var result = new UploadLayoutCsvResult();

            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) { result.Errors.Add("Experiment not found."); return result; }
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment))
            {
                result.Errors.Add("Permission denied.");
                return result;
            }

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) { result.Errors.Add("Plate layout not found."); return result; }

            List<PlateLayoutCsvRow> csvRows = new();

            try
            {
                using var reader = new StreamReader(csvStream);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
                };
                using var csv = new CsvReader(reader, config);
                csvRows = csv.GetRecords<PlateLayoutCsvRow>().ToList();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"CSV parse error: {ex.Message}");
                return result;
            }

            result.TotalRows = csvRows.Count;

            foreach (var row in csvRows)
            {
                var normalized = WellIdHelper.NormalizeWellId(row.WellId);
                if (normalized == null)
                {
                    result.Errors.Add($"Invalid well ID: {row.WellId}");
                    result.SkippedRows++;
                    continue;
                }

                var well = layout.PlateWells.FirstOrDefault(w => w.WellId == normalized);
                if (well == null) { result.SkippedRows++; continue; }

                ApplyWellUpdate(well, new UpdateWellRequest
                {
                    SampleName = row.SampleName,
                    TargetGene = row.TargetGene,
                    ReferenceGene = row.ReferenceGene,
                    SampleType = row.SampleType,
                    ReplicateGroup = row.ReplicateGroup
                });
                well.UpdatedAt = DateTime.UtcNow;
                result.SuccessRows++;
            }

            if (result.SuccessRows > 0)
            {
                MarkPlateDesignedIfNeeded(experiment);
                await _db.SaveChangesAsync();
            }

            return result;
        }

        public async Task<int> ExcludeWellsAsync(Guid experimentId, ExcludeWellsRequest request, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return 0;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return 0;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return 0;

            int count = 0;
            foreach (var rawId in request.WellIds)
            {
                var normalized = WellIdHelper.NormalizeWellId(rawId);
                if (normalized == null) continue;

                var well = layout.PlateWells.FirstOrDefault(w => w.WellId == normalized);
                if (well == null) continue;

                well.IsExcluded = true;
                well.ExclusionReason = request.Reason;
                well.UpdatedAt = DateTime.UtcNow;
                count++;
            }

            if (count > 0) await _db.SaveChangesAsync();
            return count;
        }

        public async Task<int> IncludeWellsAsync(Guid experimentId, List<string> wellIds, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return 0;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return 0;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return 0;

            int count = 0;
            foreach (var rawId in wellIds)
            {
                var normalized = WellIdHelper.NormalizeWellId(rawId);
                if (normalized == null) continue;

                var well = layout.PlateWells.FirstOrDefault(w => w.WellId == normalized);
                if (well == null) continue;

                well.IsExcluded = false;
                well.ExclusionReason = null;
                well.UpdatedAt = DateTime.UtcNow;
                count++;
            }

            if (count > 0) await _db.SaveChangesAsync();
            return count;
        }

        public async Task<int> ClearWellsAsync(Guid experimentId, ClearWellsRequest request, Guid currentUserId)
        {
            var experiment = await _db.Experiments.FirstOrDefaultAsync(e => e.ExperimentId == experimentId);
            if (experiment == null) return 0;
            if (!await _permissions.CanEditExperimentAsync(currentUserId, experiment)) return 0;

            var layout = await _db.PlateLayouts
                .Include(pl => pl.PlateWells)
                .FirstOrDefaultAsync(pl => pl.ExperimentId == experimentId);

            if (layout == null) return 0;

            IEnumerable<PlateWell> targets;
            if (request.WellIds == null || request.WellIds.Count == 0)
            {
                targets = layout.PlateWells;
            }
            else
            {
                var normalized = request.WellIds
                    .Select(id => WellIdHelper.NormalizeWellId(id))
                    .Where(id => id != null)
                    .ToHashSet();
                targets = layout.PlateWells.Where(w => normalized.Contains(w.WellId));
            }

            int count = 0;
            foreach (var well in targets.ToList())
            {
                well.SampleName = null;
                well.TargetGene = null;
                well.ReferenceGene = null;
                well.SampleType = null;
                well.ReplicateGroup = null;
                well.IsExcluded = false;
                well.ExclusionReason = null;
                well.UpdatedAt = DateTime.UtcNow;
                count++;
            }

            if (count > 0) await _db.SaveChangesAsync();
            return count;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void ApplyWellUpdate(PlateWell well, UpdateWellRequest req)
        {
            if (req.SampleName != null) well.SampleName = req.SampleName;
            if (req.TargetGene != null) well.TargetGene = req.TargetGene;
            if (req.ReferenceGene != null) well.ReferenceGene = req.ReferenceGene;
            if (req.SampleType != null) well.SampleType = req.SampleType;
            if (req.ReplicateGroup != null) well.ReplicateGroup = req.ReplicateGroup;
        }

        private static PlateWellDto MapToDto(PlateWell well, int rowIndex, int colIndex)
        {
            return new PlateWellDto
            {
                PlateWellId = well.PlateWellId,
                PlateLayoutId = well.PlateLayoutId,
                WellId = well.WellId,
                SampleName = well.SampleName,
                TargetGene = well.TargetGene,
                ReferenceGene = well.ReferenceGene,
                SampleType = well.SampleType,
                ReplicateGroup = well.ReplicateGroup,
                CtValue = well.CtValue,
                IsExcluded = well.IsExcluded,
                ExclusionReason = well.ExclusionReason,
                RowIndex = rowIndex,
                ColIndex = colIndex
            };
        }

        private static void MarkPlateDesignedIfNeeded(Experiment experiment)
        {
            if (experiment.Status == ExperimentStatus.Draft)
            {
                experiment.Status = ExperimentStatus.PlateDesigned;
                experiment.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
