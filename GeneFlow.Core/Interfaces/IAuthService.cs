using GeneFlow.Core.DTOs.Auth;

namespace GeneFlow.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
}
