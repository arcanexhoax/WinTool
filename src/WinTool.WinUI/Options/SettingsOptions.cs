namespace WinTool.WinUI.Options;

public class SettingsOptions
{
    public bool WindowsStartupEnabled { get; set; } = true;
    public bool AlwaysRunAsAdmin { get; set; }
    public string? Language { get; set; }
    public AppTheme AppTheme { get; set; } = AppTheme.System;
    public AnimationMode AnimationMode { get; set; } = AnimationMode.Auto;
    public NotificationOptions Notifications { get; set; } = new();
}

public class NotificationOptions
{
    public bool NewVersions { get; set; } = true;
}

public enum AppTheme
{
    System,
    Light,
    Dark
}

public enum AnimationMode
{
    Auto,
    On,
    Off
}
