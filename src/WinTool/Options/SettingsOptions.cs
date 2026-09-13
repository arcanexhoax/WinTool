using System;
using WinTool.ViewModels.Settings;

namespace WinTool.Options;

public class SettingsOptions
{
    public bool WindowsStartupEnabled { get; set; } = true;
    public bool AlwaysRunAsAdmin { get; set; }
    public string? Language { get; set; }
    public AppTheme AppTheme { get; set; } = AppTheme.System;
    public AnimationMode AnimationMode { get; set; } = AnimationMode.Auto;
    public UpdateOptions Update { get; set; } = new();
}

public class UpdateOptions
{
    public Version AvailableVersion { get; set; } = new(0, 0, 0);
}
