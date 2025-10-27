using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;

namespace WinTool.Services;

public class Shell(StaThreadService staThreadService)
{
    private readonly StaThreadService _staThreadService = staThreadService;

    public bool IsActive
    {
        get
        {
            try
            {
                var handle = NativeMethods.GetForegroundWindow();
                var className = NativeMethods.GetClassName(handle);

                return className is "CabinetWClass" or "ExploreWClass";
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to check if Explorer is active: {ex.Message}");
                return false;
            }
        }
    }

    public string? GetActiveExplorerPath()
    {
        return _staThreadService.Invoke(GetActiveExplorerPathInternal);
    }

    public List<string> GetSelectedItemsPaths()
    {
        return _staThreadService.Invoke(GetSelectedItemsPathsInternal);
    }

    private string? GetActiveExplorerPathInternal()
    {
        IntPtr handle = NativeMethods.GetForegroundWindow();
        InternetExplorer? window = GetActiveShellWindow(handle);

        if (window is null)
            return null;

        return new Uri(window.LocationURL).LocalPath;
    }

    private List<string> GetSelectedItemsPathsInternal()
    {
        List<string> selectedItemsPaths = [];

        IntPtr handle = NativeMethods.GetForegroundWindow();
        InternetExplorer? window = GetActiveShellWindow(handle);

        if (window is null)
            return selectedItemsPaths;

        FolderItems folderItems = ((IShellFolderViewDual2)window.Document).SelectedItems();

        foreach (var folderItem in folderItems)
        {
            string selectedItemPath = ((FolderItem)folderItem).Path;

            if (!string.IsNullOrEmpty(selectedItemPath))
                selectedItemsPaths.Add(selectedItemPath);
        }

        return selectedItemsPaths;
    }

    private InternetExplorer? GetActiveShellWindow(IntPtr handle)
    {
        // check if current Windows version is 11
        if (Environment.OSVersion.Version.Build >= 22000)
            return GetActiveShellWindow11(handle);
        else
            return GetActiveShellWindow10(handle);
    }

    private InternetExplorer? GetActiveShellWindow10(IntPtr handle)
    {
        ShellWindows shellWindows = new();

        foreach (InternetExplorer window in shellWindows)
        {
            if (window.HWND == handle.ToInt32())
                return window;
        }

        return null;
    }

    private InternetExplorer? GetActiveShellWindow11(IntPtr handle)
    {
        string? windowTitle = NativeMethods.GetWindowText(handle);

        if (windowTitle is null or [])
        {
            Trace.WriteLine("Failed to get window title of current Explorer window");
            return null;
        }

        ShellWindows shellWindows = new();
        List<InternetExplorer> tabs = [];

        // The explorer window in Windows 11 supports multiple tabs, these tabs will have the same window handle
        foreach (InternetExplorer window in shellWindows)
        {
            if (window.HWND == handle.ToInt32() && window.LocationName is not (null or []))
                tabs.Add(window);
        }

        if (tabs.Count == 0)
            return null;
        if (tabs.Count == 1)
            return tabs[0];

        // To find proper tab we need to match location name of each tab (folder name or its special name, like "Downloads")
        // with the window title (format "Folder - File Explorer" or "Folder and X more tabs - File Explorer")
        // If user has multiple tabs opened in the same folder we will select the first one
        return tabs.MaxBy(t =>
        {
            var locationName = t.LocationName;
            return windowTitle.StartsWith(locationName) ? locationName.Length : 0;
        });
    }
}
