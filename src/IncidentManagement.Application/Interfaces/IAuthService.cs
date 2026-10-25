using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default);
}
