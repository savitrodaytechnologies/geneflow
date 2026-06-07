using GeneFlow.Core.DTOs.Dashboard;
using GeneFlow.Core.Enums;
using GeneFlow.Infrastructure.Data;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeneFlow.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly GeneFlowDbContext _db;
    private readonly CurrentUserService _currentUser;

    public DashboardController(GeneFlowDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("mobile")]
    public async Task<IActionResult> GetMobileDashboard()
    {
        var userId = _currentUser.UserId;
        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var myDrafts = await _db.Experiments
            .CountAsync(e => e.OwnerUserId == userId && e.Status == ExperimentStatus.Draft);

        var pendingAnalysis = await _db.Experiments
            .CountAsync(e => e.LabId == labId && e.Status == ExperimentStatus.DataUploaded);

        var withWarnings = await _db.Experiments
            .CountAsync(e => e.LabId == labId
                && e.AnalysisRuns.Any(ar => ar.AnalysisWarnings.Any()));

        var recentReports = await _db.ExportFiles
            .CountAsync(ef => ef.Experiment.LabId == labId
                && ef.CreatedAt >= DateTime.UtcNow.AddDays(-7));

        var recentExperiments = await _db.Experiments
            .AsNoTracking()
            .Where(e => e.LabId == labId)
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .Take(10)
            .Select(e => new RecentExperimentDto
            {
                ExperimentId = e.ExperimentId,
                ExperimentName = e.ExperimentName,
                ProjectName = e.Project != null ? e.Project.ProjectName : null,
                OwnerName = e.OwnerUser.FullName,
                Status = e.Status.ToString(),
                WarningCount = e.AnalysisRuns
                    .SelectMany(ar => ar.AnalysisWarnings)
                    .Count(),
                LastUpdated = e.UpdatedAt ?? e.CreatedAt
            })
            .ToListAsync();

        return Ok(new MobileDashboardDto
        {
            Summary = new DashboardSummaryDto
            {
                MyDrafts = myDrafts,
                PendingAnalysis = pendingAnalysis,
                ExperimentsWithWarnings = withWarnings,
                RecentReports = recentReports
            },
            RecentExperiments = recentExperiments
        });
    }
}
