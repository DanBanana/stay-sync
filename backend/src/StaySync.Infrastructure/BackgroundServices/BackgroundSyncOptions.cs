namespace StaySync.Infrastructure.BackgroundServices;

public class BackgroundSyncOptions
{
    public const string Section = "BackgroundSync";
    public int IntervalMinutes { get; set; } = 60;
}
