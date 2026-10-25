using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAt, UserSummaryDto User);

public record UserSummaryDto(Guid Id, string Email, string DisplayName, UserRole Role);

public record UserDto(Guid Id, string Email, string DisplayName, UserRole Role, DateTime CreatedAt, bool IsActive);
