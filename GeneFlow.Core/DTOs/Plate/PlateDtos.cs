using System;
using System.Collections.Generic;

namespace GeneFlow.Core.DTOs.Plate
{
    public class PlateWellDto
    {
        public Guid PlateWellId { get; set; }
        public Guid PlateLayoutId { get; set; }
        public string WellId { get; set; } = string.Empty; // e.g. "A01"
        public string? SampleName { get; set; }
        public string? TargetGene { get; set; }
        public string? ReferenceGene { get; set; }
        public string? SampleType { get; set; }  // "sample", "control", "NTC", "standard"
        public string? ReplicateGroup { get; set; }
        public decimal? CtValue { get; set; }
        public bool IsExcluded { get; set; }
        public string? ExclusionReason { get; set; }
        public int RowIndex { get; set; }  // 0-7 (A-H)
        public int ColIndex { get; set; }  // 0-11 (01-12)
    }

    public class PlateGridDto
    {
        public Guid PlateLayoutId { get; set; }
        public Guid ExperimentId { get; set; }
        public string LayoutName { get; set; } = string.Empty;
        public int TotalWells { get; set; }
        public int FilledWells { get; set; }
        public int ExcludedWells { get; set; }
        // 8 rows x 12 cols grid
        public List<List<PlateWellDto>> Grid { get; set; } = new();
    }

    public class UpdateWellRequest
    {
        public string? SampleName { get; set; }
        public string? TargetGene { get; set; }
        public string? ReferenceGene { get; set; }
        public string? SampleType { get; set; }
        public string? ReplicateGroup { get; set; }
    }

    public class BulkUpdateWellsRequest
    {
        public List<SingleWellUpdate> Wells { get; set; } = new();
    }

    public class SingleWellUpdate
    {
        public string WellId { get; set; } = string.Empty; // e.g. "A01"
        public string? SampleName { get; set; }
        public string? TargetGene { get; set; }
        public string? ReferenceGene { get; set; }
        public string? SampleType { get; set; }
        public string? ReplicateGroup { get; set; }
    }

    public class QuickFillRequest
    {
        public string FromWell { get; set; } = string.Empty;  // e.g. "A01"
        public string ToWell { get; set; } = string.Empty;    // e.g. "B06"
        public string? SampleName { get; set; }
        public string? TargetGene { get; set; }
        public string? ReferenceGene { get; set; }
        public string? SampleType { get; set; }
        public string? ReplicateGroup { get; set; }
    }

    public class ExcludeWellsRequest
    {
        public List<string> WellIds { get; set; } = new(); // e.g. ["A01","A02"]
        public string? Reason { get; set; }
    }

    public class ClearWellsRequest
    {
        public List<string> WellIds { get; set; } = new(); // empty list = clear all
    }

    public class PlateLayoutCsvRow
    {
        public string WellId { get; set; } = string.Empty;
        public string? SampleName { get; set; }
        public string? TargetGene { get; set; }
        public string? ReferenceGene { get; set; }
        public string? SampleType { get; set; }
        public string? ReplicateGroup { get; set; }
    }

    public class UploadLayoutCsvResult
    {
        public int TotalRows { get; set; }
        public int SuccessRows { get; set; }
        public int SkippedRows { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
