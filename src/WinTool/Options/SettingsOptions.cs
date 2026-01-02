using WinTool.ViewModels.Settings;

namespace WinTool.Options;

public class SettingsOptions
{
    public bool WindowsStartupEnabled { get; set; } = true;
    public bool AlwaysRunAsAdmin { get; set; }

    public AppTheme AppTheme { get; set; } = AppTheme.System;
}
