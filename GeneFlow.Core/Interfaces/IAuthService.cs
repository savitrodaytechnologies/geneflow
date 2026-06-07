using GeneFlow.Core.DTOs.Auth;
using GeneFlow.Core.DTOs.Lab;

namespace GeneFlow.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task<RegisterLabResponse> RegisterLabAsync(RegisterLabRequest request);
    Task<LabMemberDto> AddLabUserAsync(Guid labId, Guid requestingUserId, AddLabUserRequest request);
    Task<List<LabMemberDto>> GetLabMembersAsync(Guid labId);
    Task<bool> DeactivateLabUserAsync(Guid labId, Guid targetUserId, Guid requestingUserId);
}
