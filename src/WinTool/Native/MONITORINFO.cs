using System.Runtime.InteropServices;

namespace WinTool.Native;

[StructLayout(LayoutKind.Sequential)]
public struct MONITORINFO
{
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public int dwFlags;
}
