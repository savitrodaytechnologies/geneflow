using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GeneFlow.Infrastructure.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetClaim(string type) =>
        _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == type)?.Value;

    public Guid UserId
    {
        get
        {
            var value = GetClaim(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public Guid? LabId
    {
        get
        {
            var value = GetClaim("labId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? LabRole => GetClaim("labRole");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
