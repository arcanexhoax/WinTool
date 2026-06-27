using Microsoft.Extensions.Logging;
using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using WinTool.Models;
using WinTool.Native;
using WinTool.Native.Shell;
using IServiceProvider = WinTool.Native.Shell.IServiceProvider;

namespace WinTool.Services;

using ShellWindow = (string? Path, IShellBrowser Browser, IShellFolderViewDual2 View);

public class Shell(ILogger<Shell> logger, StaThreadService staThreadService)
{
    private const int SW_SHOWNORMAL = 1;
    private const int SWC_DESKTOP = 8;
    private const int SWFO_NEEDDISPATCH = 1;

    private readonly ILogger _logger = logger;
    private readonly StaThreadService _staThreadService = staThreadService;
    private readonly Guid SID_STopLevelBrowser = new("4C96BE40-915C-11CF-99D3-00AA004AE837");

    private IShellDispatch2? _shellDispatch;

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
            if (GetActiveShellWindow() is not (var path, _, _))
                return null;

            return path;
        });
    }

    public List<ItemInfo> GetSelectedItems()
    {
        return _staThreadService.Invoke(() =>
        {
            if (GetActiveShellWindow() is not (_, var browser, _))
                return [];

            return GetSelectedItems(browser);
        });
    }

    public bool BeginRename(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = Path.GetFullPath(path);

        return _staThreadService.Invoke(() =>
        {
            if (GetActiveShellWindow() is not (_, _, var view))
                return false;

            return BeginRename(view, fullPath);
        });
    }

    public void StartProcessUnelevated(string fileName, string? args = null)
    {
        _staThreadService.Invoke(() =>
        {
            var shellDispatch = GetShellDispatch();

            try
            {
                shellDispatch.ShellExecute(fileName, args ?? string.Empty, string.Empty, string.Empty, SW_SHOWNORMAL);
            }
            catch (Exception ex) when (ex is COMException or InvalidOperationException)
            {
                _shellDispatch = null;
                shellDispatch = GetShellDispatch();
                shellDispatch.ShellExecute(fileName, args ?? string.Empty, string.Empty, string.Empty, SW_SHOWNORMAL);
            }
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
                return (uri?.LocalPath, shellBrowser, shellFolderView);
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

        var webBrowser = (IWebBrowserApp)shellWindows.FindWindowSW(ref obj1, ref obj2, SWC_DESKTOP, out int pHWND, SWFO_NEEDDISPATCH);
        var serviceProvider = (IServiceProvider)webBrowser;
        var shellBrowser = (IShellBrowser)serviceProvider.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID);

        if (webBrowser.Document is not IShellFolderViewDual2 shellFolderView)
            return null;

        return (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shellBrowser, shellFolderView);
    }

    private IShellDispatch2 GetShellDispatch()
    {
        if (_shellDispatch != null)
            return _shellDispatch;

        var shell = new Shell32.Shell();
        ShellWindows shellWindows = shell.Windows();

        object? location = null;
        object? root = null;
        var webBrowserObj = shellWindows.FindWindowSW(ref location, ref root, SWC_DESKTOP, out _, SWFO_NEEDDISPATCH);

        if (webBrowserObj is not IWebBrowserApp webBrowser
            || webBrowser.Document is not IShellFolderViewDual2 shellFolderView
            || shellFolderView.Application is not IShellDispatch2 shellDispatch)
        {
            throw new InvalidOperationException("Unable to get the desktop shell.");
        }

        return shellDispatch;
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
                _logger.LogWarning(ex, "Error getting selected item at index {Index}", i);
            }

            if (path is null)
                continue;

            itemsPaths.Add(new ItemInfo(name, path));
        }

        return itemsPaths;
    }

    private bool BeginRename(IShellFolderViewDual2 shellFolderView, string path)
    {
        var fileName = Path.GetFileName(path);
        FolderItem item = shellFolderView.Folder.ParseName(fileName);

        if (item is null || !string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase))
            return false;

        object itemObject = item;
        var flags = SVSI.EDIT | SVSI.DESELECTOTHERS | SVSI.ENSUREVISIBLE | SVSI.FOCUSED;
        shellFolderView.SelectItem(ref itemObject, (int)flags);

        return true;
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
