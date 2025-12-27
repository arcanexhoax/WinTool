using System;
using System.Runtime.InteropServices;
using WinTool.Native;

namespace WinTool.Utils;

public static class DpiUtils
{
    private const int MONITOR_DEFAULTTONEAREST = 2;
    public const float DefaultDpiX = 96;
    public const float DefaultDpiY = 96;

    private delegate int GetDpiForWindowFn(nint hwnd);
    private delegate int GetDpiForMonitorFn(nint hmonitor, MonitorDpiType dpiType, out int dpiX, out int dpiY);

    // you should always use this one and it will fallback if necessary
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow
    public static int GetDpiForWindow(nint hwnd)
    {
        var h = NativeMethods.LoadLibrary("user32.dll");
        var ptr = NativeMethods.GetProcAddress(h, "GetDpiForWindow"); // Windows 10 1607

        if (ptr == nint.Zero)
            return GetDpiForNearestMonitor(hwnd);

        return Marshal.GetDelegateForFunctionPointer<GetDpiForWindowFn>(ptr)(hwnd);
    }

    public static int GetDpiForNearestMonitor(nint hwnd) => GetDpiForMonitor(GetNearestMonitorFromWindow(hwnd));

    public static int GetDpiForNearestMonitor(int x, int y) => GetDpiForMonitor(GetNearestMonitorFromPoint(x, y));

    public static int GetDpiForMonitor(nint monitor, MonitorDpiType type = MonitorDpiType.Effective)
    {
        var h = NativeMethods.LoadLibrary("shcore.dll");
        var ptr = NativeMethods.GetProcAddress(h, "GetDpiForMonitor"); // Windows 8.1

        if (ptr == nint.Zero)
            return GetDpiForDesktop();

        int hr = Marshal.GetDelegateForFunctionPointer<GetDpiForMonitorFn>(ptr)(monitor, type, out int x, out int y);

        if (hr < 0)
            return GetDpiForDesktop();

        return x;
    }

    public static int GetDpiForDesktop()
    {
        int hr = NativeMethods.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, typeof(ID2D1Factory).GUID, nint.Zero, out ID2D1Factory factory);

        if (hr < 0)
            return 96;

        factory.GetDesktopDpi(out float x, out float y); // Windows 7
        Marshal.ReleaseComObject(factory);
        return (int)x;
    }

    public static nint GetDesktopMonitor() => GetNearestMonitorFromWindow(NativeMethods.GetDesktopWindow());

    public static nint GetShellMonitor() => GetNearestMonitorFromWindow(NativeMethods.GetShellWindow());

    public static nint GetNearestMonitorFromWindow(nint hwnd) => NativeMethods.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

    public static nint GetNearestMonitorFromPoint(int x, int y) => NativeMethods.MonitorFromPoint(new POINT { X = x, Y = y }, MONITOR_DEFAULTTONEAREST);
}

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("06152247-6f50-465a-9245-118bfd3b6007")]
public interface ID2D1Factory
{
    int ReloadSystemMetrics();

    [PreserveSig]
    void GetDesktopDpi(out float dpiX, out float dpiY);

    // the rest is not implemented as we don't need it
}

public enum MonitorDpiType
{
    Effective = 0,
    Angular = 1,
    Raw = 2,
}

public enum D2D1_FACTORY_TYPE
{
    D2D1_FACTORY_TYPE_SINGLE_THREADED = 0,
    D2D1_FACTORY_TYPE_MULTI_THREADED = 1,
}
