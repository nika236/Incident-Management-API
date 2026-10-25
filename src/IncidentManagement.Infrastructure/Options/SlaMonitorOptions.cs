namespace IncidentManagement.Infrastructure.Options;

public class SlaMonitorOptions
{
    public const string SectionName = "SlaMonitor";

    public int RunIntervalMinutes { get; set; } = 5;
}
