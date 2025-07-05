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

public class Shell
{
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

    public Task<string?> GetActiveExplorerPathAsync()
    {
        TaskCompletionSource<string?> tcs = new();
        // use new thread because it is unable to get shell windows from MTA thread
        Thread t = new(() =>
        {
            try
            {
                string? path = GetActiveExplorerPath();
                tcs.TrySetResult(path);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        return tcs.Task;
    }

    public Task<List<string>> GetSelectedItemsPathsAsync()
    {
        TaskCompletionSource<List<string>> tcs = new();
        // use new thread because it is unable to get shell windows from MTA thread
        Thread t = new(() =>
        {
            try
            {
                var paths = GetSelectedItemsPaths();
                tcs.TrySetResult(paths);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        return tcs.Task;
    }

    private string? GetActiveExplorerPath()
    {
        IntPtr handle = NativeMethods.GetForegroundWindow();
        InternetExplorer? window = GetActiveShellWindow(handle);

        if (window is null)
            return null;

        return new Uri(window.LocationURL).LocalPath;
    }

    private List<string> GetSelectedItemsPaths()
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

        foreach (InternetExplorer window in shellWindows)
        {
            if (window.HWND == handle.ToInt32() && window.LocationURL is not null and not [])
                tabs.Add(window);
        }

        if (tabs.Count == 0)
            return null;
        if (tabs.Count == 1)
            return tabs[0];

        // The explorer window in Windows 11 supports multiple tabs, these tabs will have the same window handle, so we need to compare 
        // the title (folder name in format "Folder - File Explorer" or "Folder and X more tabs - File Explorer") of the current explorer window with
        // the local path of each tab of this window. So this way is not 100% accurate and doesn't work with Desktop/Downloads/Pictures etc folders
        return tabs.MaxBy(t =>
        {
            if (!Uri.TryCreate(t.LocationURL, UriKind.RelativeOrAbsolute, out var uri))
                return 0;

            var dirName = Path.GetFileName(uri.LocalPath);
            return windowTitle.StartsWith(dirName) ? dirName.Length : 0;
        });
    }
}
