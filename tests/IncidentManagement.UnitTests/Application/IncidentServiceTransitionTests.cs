using FluentAssertions;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Domain.Services;

namespace IncidentManagement.UnitTests.Application;

public class IncidentServiceTransitionTests
{
    [Fact]
    public void Open_To_Resolved_IsNotAllowed()
    {
        IncidentStatusTransitionValidator.CanTransition(IncidentStatus.Open, IncidentStatus.Resolved)
            .Should().BeFalse();
    }

    [Fact]
    public void ComputeTargets_UsesCreatedAtAsBaseline()
    {
        var created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var (ack, resolve) = SlaCalculator.ComputeTargets(IncidentSeverity.SEV3, created);
        ack.Should().Be(created.AddHours(2));
        resolve.Should().Be(created.AddHours(24));
    }
}
