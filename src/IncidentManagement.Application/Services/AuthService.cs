using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IApplicationDbContext db, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid email or password.");

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);
        var summary = new UserSummaryDto(user.Id, user.Email, user.DisplayName, user.Role);
        return new LoginResponse(token, expiresAt, summary);
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new UserDto(u.Id, u.Email, u.DisplayName, u.Role, u.CreatedAt, u.IsActive))
            .ToListAsync(cancellationToken);
    }
}
