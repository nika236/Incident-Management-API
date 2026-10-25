using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();
    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
}
