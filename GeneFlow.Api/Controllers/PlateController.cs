using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeneFlow.Core.DTOs.Plate;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeneFlow.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/experiments/{experimentId}/plate")]
    public class PlateController : ControllerBase
    {
        private readonly IPlateService _plateService;
        private readonly CurrentUserService _currentUser;

        public PlateController(IPlateService plateService, CurrentUserService currentUser)
        {
            _plateService = plateService;
            _currentUser = currentUser;
        }

        /// <summary>GET /api/experiments/{experimentId}/plate — full 8×12 grid</summary>
        [HttpGet]
        public async Task<IActionResult> GetGrid(Guid experimentId)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var grid = await _plateService.GetPlateGridAsync(experimentId, userId);
            if (grid == null) return NotFound();
            return Ok(grid);
        }

        /// <summary>GET /api/experiments/{experimentId}/plate/list — flat list of 96 wells</summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetList(Guid experimentId)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var wells = await _plateService.GetPlateListAsync(experimentId, userId);
            return Ok(wells);
        }

        /// <summary>PUT /api/experiments/{experimentId}/plate/wells/{wellId} — update single well</summary>
        [HttpPut("wells/{wellId}")]
        public async Task<IActionResult> UpdateWell(Guid experimentId, string wellId, [FromBody] UpdateWellRequest request)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _plateService.UpdateWellAsync(experimentId, wellId, request, userId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/bulk-update — update multiple wells</summary>
        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdate(Guid experimentId, [FromBody] BulkUpdateWellsRequest request)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _plateService.BulkUpdateWellsAsync(experimentId, request, userId);
            return Ok(new { updatedWells = count });
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/quick-fill — fill a range of wells</summary>
        [HttpPost("quick-fill")]
        public async Task<IActionResult> QuickFill(Guid experimentId, [FromBody] QuickFillRequest request)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _plateService.QuickFillAsync(experimentId, request, userId);
            return Ok(new { filledWells = count });
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/upload-csv — upload plate layout CSV</summary>
        [HttpPost("upload-csv")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCsv(Guid experimentId, IFormFile file)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only CSV files are supported." });

            var result = await _plateService.UploadLayoutCsvAsync(experimentId, file.OpenReadStream(), file.FileName, userId);
            return Ok(result);
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/exclude — exclude wells</summary>
        [HttpPost("exclude")]
        public async Task<IActionResult> ExcludeWells(Guid experimentId, [FromBody] ExcludeWellsRequest request)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _plateService.ExcludeWellsAsync(experimentId, request, userId);
            return Ok(new { excludedWells = count });
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/include — un-exclude wells</summary>
        [HttpPost("include")]
        public async Task<IActionResult> IncludeWells(Guid experimentId, [FromBody] List<string> wellIds)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _plateService.IncludeWellsAsync(experimentId, wellIds, userId);
            return Ok(new { includedWells = count });
        }

        /// <summary>POST /api/experiments/{experimentId}/plate/clear — clear well data</summary>
        [HttpPost("clear")]
        public async Task<IActionResult> ClearWells(Guid experimentId, [FromBody] ClearWellsRequest request)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _plateService.ClearWellsAsync(experimentId, request, userId);
            return Ok(new { clearedWells = count });
        }
    }
}
