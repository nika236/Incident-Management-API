using FluentAssertions;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Domain.Services;

namespace IncidentManagement.UnitTests.Domain;

public class IncidentStatusTransitionValidatorTests
{
    [Theory]
    [InlineData(IncidentStatus.Open, IncidentStatus.Acknowledged, true)]
    [InlineData(IncidentStatus.Open, IncidentStatus.Closed, true)]
    [InlineData(IncidentStatus.Open, IncidentStatus.Resolved, false)]
    [InlineData(IncidentStatus.Resolved, IncidentStatus.Closed, true)]
    [InlineData(IncidentStatus.Closed, IncidentStatus.Open, false)]
    public void CanTransition_ReturnsExpected(IncidentStatus from, IncidentStatus to, bool expected)
    {
        IncidentStatusTransitionValidator.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void SlaCalculator_Sev1_HasShortestWindows()
    {
        var sev1 = SlaCalculator.GetWindows(IncidentSeverity.SEV1);
        var sev4 = SlaCalculator.GetWindows(IncidentSeverity.SEV4);
        sev1.AcknowledgeWindow.Should().BeLessThan(sev4.AcknowledgeWindow);
        sev1.ResolveWindow.Should().BeLessThan(sev4.ResolveWindow);
    }
}
