using Microsoft.UI.Xaml;

namespace WinTool.Options;

public class SettingsOptions
{
    public bool WindowsStartupEnabled { get; set; }
    public bool AlwaysRunAsAdmin { get; set; }

    public ElementTheme AppTheme { get; set; } = ElementTheme.Default;
}
