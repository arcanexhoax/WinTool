using System;

namespace WinTool.Native;

[Flags]
public enum DWMSBT : uint
{
    /// <summary>
    /// Automatically selects backdrop effect.
    /// </summary>
    AUTO = 0,

    /// <summary>
    /// Turns off the backdrop effect.
    /// </summary>
    DISABLE = 1,

    /// <summary>
    /// Sets Mica effect with generated wallpaper tint.
    /// </summary>
    MAINWINDOW = 2,

    /// <summary>
    /// Sets acrlic effect.
    /// </summary>
    TRANSIENTWINDOW = 3,

    /// <summary>
    /// Sets blurred wallpaper effect, like Mica without tint.
    /// </summary>
    TABBEDWINDOW = 4,
}