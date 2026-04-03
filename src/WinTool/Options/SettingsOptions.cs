using WinTool.ViewModels.Settings;

namespace WinTool.Options;

public class SettingsOptions
{
    public bool WindowsStartupEnabled { get; set; } = true;
    public bool AlwaysRunAsAdmin { get; set; }
    public string? Language { get; set; }
    public AppTheme AppTheme { get; set; } = AppTheme.System;
    public AnimationMode AnimationMode { get; set; } = AnimationMode.Auto;
}
