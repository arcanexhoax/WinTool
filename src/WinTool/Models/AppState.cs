using System;

namespace WinTool.Models;

public class AppState(Version version)
{
    public Version Version { get; } = version;
    public bool IsBackgroundMode { get; set; } = true;

    public AppState() : this(typeof(AppState).Assembly.GetName().Version ?? new Version(0, 0, 0))
    {
    }
}
