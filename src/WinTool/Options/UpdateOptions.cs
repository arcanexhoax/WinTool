using System;

namespace WinTool.Options;

public class UpdateOptions
{
    public TimeSpan InitialCheckDelay { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromHours(24);
    public Version AvailableVersion { get; set; } = new(0, 0, 0);
}
