using System;
using System.IO;
using System.Threading.Tasks;
using GeneFlow.Core.DTOs.Plate;

namespace GeneFlow.Core.Interfaces
{
    public interface IPlateService
    {
        // Get full plate grid (8x12 grid format)
        Task<PlateGridDto?> GetPlateGridAsync(Guid experimentId, Guid currentUserId);

        // Get flat list of all 96 wells
        Task<System.Collections.Generic.List<PlateWellDto>> GetPlateListAsync(Guid experimentId, Guid currentUserId);

        // Update a single well by wellId string (e.g. "A01")
        Task<PlateWellDto?> UpdateWellAsync(Guid experimentId, string wellId, UpdateWellRequest request, Guid currentUserId);

        // Bulk update multiple wells
        Task<int> BulkUpdateWellsAsync(Guid experimentId, BulkUpdateWellsRequest request, Guid currentUserId);

        // Quick fill: fill a range of wells with the same data
        Task<int> QuickFillAsync(Guid experimentId, QuickFillRequest request, Guid currentUserId);

        // Upload plate layout from CSV stream
        Task<UploadLayoutCsvResult> UploadLayoutCsvAsync(Guid experimentId, Stream csvStream, string fileName, Guid currentUserId);

        // Exclude wells from analysis
        Task<int> ExcludeWellsAsync(Guid experimentId, ExcludeWellsRequest request, Guid currentUserId);

        // Include (un-exclude) wells
        Task<int> IncludeWellsAsync(Guid experimentId, System.Collections.Generic.List<string> wellIds, Guid currentUserId);

        // Clear well data (reset to empty)
        Task<int> ClearWellsAsync(Guid experimentId, ClearWellsRequest request, Guid currentUserId);
    }
}
