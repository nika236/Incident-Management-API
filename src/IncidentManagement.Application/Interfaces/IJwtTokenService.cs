using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
