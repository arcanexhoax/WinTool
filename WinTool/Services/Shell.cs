using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinTool.Models;
using WinTool.Native;
using WinTool.Native.Shell;
using IServiceProvider = WinTool.Native.Shell.IServiceProvider;

namespace WinTool.Services;

using ShellWindow = (string? Path, IShellBrowser Browser);

public class Shell(StaThreadService staThreadService)
{
    private const int SWC_DESKTOP = 8;
    private const int SWFO_NEEDDISPATCH = 1;

    private readonly StaThreadService _staThreadService = staThreadService;
    private readonly Guid SID_STopLevelBrowser = new("4C96BE40-915C-11CF-99D3-00AA004AE837");

    public bool IsActive
    {
        get
        {
            var foregroundWindow = NativeMethods.GetForegroundWindow();
            return IsExplorer(foregroundWindow) || IsDesktop(foregroundWindow);
        }
    }

    public string? GetActiveShellPath()
    {
        return _staThreadService.Invoke(() =>
        {
            if (GetActiveShellWindow() is not (var path, _))
                return null;

            return path;
        });
    }

    public List<ItemInfo> GetSelectedItems()
    {
        return _staThreadService.Invoke(() =>
        {
            if (GetActiveShellWindow() is not (_, var browser))
                return [];

            return GetSelectedItems(browser);
        });
    }

    private ShellWindow? GetActiveShellWindow()
    {
        var handle = NativeMethods.GetForegroundWindow();

        if (IsExplorer(handle))
            return GetActiveExplorerWindow(handle);
        else if (IsDesktop(handle))
            return GetActiveDesktopWindow(handle);
        else
            return null;
    }

    private ShellWindow? GetActiveExplorerWindow(nint handle)
    {
        var activeTab = GetActiveExplorerTab(handle);
        var shell = new Shell32.Shell();
        ShellWindows shellWindows = shell.Windows();

        foreach (IWebBrowserApp webBrowser in shellWindows)
        {
            if (webBrowser.HWND != handle || webBrowser.Document is not IShellFolderViewDual2 shellFolderView)
                continue;

            var serviceProvider = (IServiceProvider)webBrowser;
            var shellBrowser = (IShellBrowser)serviceProvider.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID);
            shellBrowser.GetWindow(out IntPtr shellBrowserHandle);

            if (activeTab == shellBrowserHandle)
            {
                Uri.TryCreate(webBrowser.LocationURL, UriKind.Absolute, out var uri);
                return (uri?.LocalPath, shellBrowser);
            }
        }

        return null;
    }

    private ShellWindow? GetActiveDesktopWindow(nint handle)
    {
        var shell = new Shell32.Shell();
        ShellWindows shellWindows = shell.Windows();

        object? obj1 = null;
        object? obj2 = null;

        var serviceProvider = (IServiceProvider)shellWindows.FindWindowSW(ref obj1, ref obj2, SWC_DESKTOP, out int pHWND, SWFO_NEEDDISPATCH);
        var shellBrowser = (IShellBrowser)serviceProvider.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID);

        return (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shellBrowser);
    }

    private List<ItemInfo> GetSelectedItems(IShellBrowser shellBrowser)
    {
        if (shellBrowser.QueryActiveShellView() is not IFolderView shellView)
            return [];

        var selectionFlag = (uint)SVGIO.SELECTION;
        shellView.ItemCount(selectionFlag, out var countItems);

        if (countItems <= 0)
            return [];

        shellView.Items(selectionFlag, typeof(IShellItemArray).GUID, out var items);

        if (items is not IShellItemArray shellItemArray)
            return [];

        var count = shellItemArray.GetCount();

        if (count < 0)
            return [];

        List<ItemInfo> itemsPaths = new(count);

        for (int i = 0; i < count; i++)
        {
            string? name = null;
            string? path = null;

            try
            {
                var itemPath = shellItemArray.GetItemAt(i);
                name = itemPath.GetDisplayName(SIGDN.NORMALDISPLAY);
                path = itemPath.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error getting selected item at index {i}: {ex}");
            }

            if (path is null)
                continue;

            itemsPaths.Add(new ItemInfo(name, path));
        }

        return itemsPaths;
    }

    private nint GetActiveExplorerTab(nint handle)
    {
        var activeTab = FindChildWindow(handle, "ShellTabWindowClass");

        if (activeTab == nint.Zero)
            activeTab = FindChildWindow(handle, "TabWindowClass");

        return activeTab;
    }

    private bool IsExplorer(nint handle)
    {
        var className = NativeMethods.GetClassName(handle);
        return className is "CabinetWClass" or "ExploreWClass";
    }

    private bool IsDesktop(nint handle)
    {
        var className = NativeMethods.GetClassName(handle);

        if (className is not "Progman" and not "WorkerW")
            return false;

        return FindChildWindow(handle, "SHELLDLL_DefView") != nint.Zero;
    }

    private nint FindChildWindow(nint handle, string className)
    {
        return NativeMethods.FindWindowEx(handle, nint.Zero, className, null);
    }
}
