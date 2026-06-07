using GeneFlow.Core.DTOs.Experiment;
using GeneFlow.Core.Interfaces;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneFlow.Api.Controllers;

[ApiController]
[Route("api/experiments")]
[Authorize]
public class ExperimentController : ControllerBase
{
    private readonly IExperimentService _experimentService;
    private readonly CurrentUserService _currentUser;

    public ExperimentController(IExperimentService experimentService, CurrentUserService currentUser)
    {
        _experimentService = experimentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetExperiments()
    {
        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var experiments = await _experimentService.GetExperimentsAsync(_currentUser.UserId, labId.Value);
        return Ok(experiments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateExperiment([FromBody] CreateExperimentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExperimentName))
            return BadRequest(new { message = "Experiment name is required." });

        var labId = _currentUser.LabId;
        if (labId is null) return BadRequest(new { message = "User is not assigned to a lab." });

        var experiment = await _experimentService.CreateExperimentAsync(request, _currentUser.UserId, labId.Value);
        return CreatedAtAction(nameof(GetExperiment), new { experimentId = experiment.ExperimentId }, experiment);
    }

    [HttpGet("{experimentId}")]
    public async Task<IActionResult> GetExperiment(Guid experimentId)
    {
        var experiment = await _experimentService.GetExperimentAsync(experimentId, _currentUser.UserId);
        if (experiment is null) return NotFound();
        return Ok(experiment);
    }

    [HttpPut("{experimentId}")]
    public async Task<IActionResult> UpdateExperiment(Guid experimentId, [FromBody] UpdateExperimentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExperimentName))
            return BadRequest(new { message = "Experiment name is required." });

        var experiment = await _experimentService.UpdateExperimentAsync(experimentId, request, _currentUser.UserId);
        if (experiment is null) return NotFound();
        return Ok(experiment);
    }

    [HttpDelete("{experimentId}")]
    public async Task<IActionResult> DeleteExperiment(Guid experimentId)
    {
        var deleted = await _experimentService.DeleteExperimentAsync(experimentId, _currentUser.UserId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{experimentId}/duplicate")]
    public async Task<IActionResult> DuplicateExperiment(Guid experimentId)
    {
        var experiment = await _experimentService.DuplicateExperimentAsync(experimentId, _currentUser.UserId);
        if (experiment is null) return NotFound();
        return Ok(experiment);
    }

    [HttpPost("{experimentId}/finalize")]
    public async Task<IActionResult> FinalizeExperiment(Guid experimentId)
    {
        var success = await _experimentService.FinalizeExperimentAsync(experimentId, _currentUser.UserId);
        if (!success) return BadRequest(new { message = "Cannot finalize experiment. Check permissions or experiment status." });
        return Ok(new { message = "Experiment finalized." });
    }

    [HttpPost("{experimentId}/unlock")]
    public async Task<IActionResult> UnlockExperiment(Guid experimentId)
    {
        var success = await _experimentService.UnlockExperimentAsync(experimentId, _currentUser.UserId);
        if (!success) return BadRequest(new { message = "Cannot unlock experiment. Only Lab Admin or PI can unlock." });
        return Ok(new { message = "Experiment unlocked." });
    }
}
