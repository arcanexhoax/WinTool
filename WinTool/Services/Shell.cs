using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using WinTool.Native;
using WinTool.Native.Shell;
using IServiceProvider = WinTool.Native.Shell.IServiceProvider;

namespace WinTool.Services;

using ShellWindow = (IWebBrowserApp WebBrowserApp, IShellBrowser ShellBrowser);

public class Shell(StaThreadService staThreadService)
{
    private readonly StaThreadService _staThreadService = staThreadService;
    private readonly Guid SID_STopLevelBrowser = new("4C96BE40-915C-11CF-99D3-00AA004AE837");

    public bool IsActive => GetActiveExplorerWindowHandle() is not null;

    public string? GetActiveExplorerPath()
    {
        return _staThreadService.Invoke(() =>
        {
            if (GetActiveExplorerWindow() is not (var webBrowser, _))
                return null;

            return new Uri(webBrowser.LocationURL).LocalPath;
        });
    }

    public List<string> GetSelectedItemsPaths()
    {
        return _staThreadService.Invoke(() =>
        {
            if (GetActiveExplorerWindow() is not (_, var shellBrowser))
                return [];

            return GetSelectedItems(shellBrowser);
        });
    }

    private ShellWindow? GetActiveExplorerWindow()
    {
        if (GetActiveExplorerWindowHandle() is not nint handle)
            return null;

        var activeTab = GetActiveTab(handle);
        var shell = new Shell32.Shell();
        ShellWindows shellWindows = shell.Windows();

        foreach (IWebBrowserApp webBrowserApp in shellWindows)
        {
            if (webBrowserApp.HWND != handle || webBrowserApp.Document is not IShellFolderViewDual2 shellFolderView)
                continue;

            var serviceProvider = (IServiceProvider)webBrowserApp;
            var shellBrowser = (IShellBrowser)serviceProvider.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID);
            shellBrowser.GetWindow(out IntPtr shellBrowserHandle);

            if (activeTab == shellBrowserHandle)
                return (webBrowserApp, shellBrowser);
        }

        return null;
    }

    private List<string> GetSelectedItems(IShellBrowser shellBrowser)
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

        List<string> itemsPaths = new(count);

        for (int i = 0; i < count; i++)
        {
            var itemPath = shellItemArray.GetItemAt(i).GetDisplayName(SIGDN.SIGDN_FILESYSPATH);
            itemsPaths.Add(itemPath);
        }

        return itemsPaths;
    }

    private nint GetActiveTab(nint handle)
    {
        var activeTab = FindChildWindow(handle, "ShellTabWindowClass");

        if (activeTab == nint.Zero)
            activeTab = FindChildWindow(handle, "TabWindowClass");

        return activeTab;
    }

    private nint FindChildWindow(nint handle, string className)
    {
        return NativeMethods.FindWindowEx(handle, nint.Zero, className, null);
    }

    private nint? GetActiveExplorerWindowHandle()
    {
        var handle = NativeMethods.GetForegroundWindow();
        var className = NativeMethods.GetClassName(handle);

        return className is "CabinetWClass" or "ExploreWClass" ? handle : null;
    }
}
