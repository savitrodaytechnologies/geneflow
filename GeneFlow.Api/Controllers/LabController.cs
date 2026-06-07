using GeneFlow.Infrastructure.Data;
using GeneFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeneFlow.Core.DTOs.Lab;

namespace GeneFlow.Api.Controllers;

[ApiController]
[Route("api/labs")]
[Authorize]
public class LabController : ControllerBase
{
    private readonly GeneFlowDbContext _db;
    private readonly CurrentUserService _currentUser;

    public LabController(GeneFlowDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentLab()
    {
        var labId = _currentUser.LabId;
        if (labId is null) return NotFound(new { message = "User is not assigned to a lab." });

        var lab = await _db.Labs
            .AsNoTracking()
            .Where(l => l.LabId == labId && l.IsActive)
            .Select(l => new LabDto
            {
                LabId = l.LabId,
                LabName = l.LabName,
                InstitutionName = l.InstitutionName,
                Description = l.Description
            })
            .FirstOrDefaultAsync();

        if (lab is null) return NotFound();
        return Ok(lab);
    }

    [HttpGet("{labId}/users")]
    public async Task<IActionResult> GetLabUsers(Guid labId)
    {
        var members = await _db.LabUsers
            .AsNoTracking()
            .Where(lu => lu.LabId == labId && lu.IsActive)
            .Select(lu => new LabMemberDto
            {
                UserId = lu.UserId,
                FullName = lu.User.FullName,
                Email = lu.User.Email,
                LabRole = lu.LabRole.ToString(),
                IsActive = lu.IsActive
            })
            .OrderBy(m => m.FullName)
            .ToListAsync();

        return Ok(members);
    }
}
