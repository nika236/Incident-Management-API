namespace IncidentManagement.Infrastructure.Options;

public class HousekeepingOptions
{
    public const string SectionName = "Housekeeping";

    public int AutoCloseResolvedAfterDays { get; set; } = 7;
    public int RunIntervalMinutes { get; set; } = 60;
}
