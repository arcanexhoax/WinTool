using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;

namespace WinTool.Services
{
    public class Shell
    {
        public Task<string?> GetActiveExplorerPathAsync()
        {
            TaskCompletionSource<string?> tcs = new();
            // use new thread because it is unable to get shell windows from MTA thread
            Thread t = new(() =>
            {
                string? path = GetActiveExplorerPath();
                tcs.TrySetResult(path);
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
                var paths = GetSelectedItemsPaths();
                tcs.TrySetResult(paths);
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            return tcs.Task;
        }

        private string? GetActiveExplorerPath()
        {
            try
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();
                InternetExplorer? window = GetActiveShellWindow(handle);

                if (window is null)
                    return null;

                return new Uri(window.LocationURL).LocalPath;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        private List<string> GetSelectedItemsPaths()
        {
            List<string> selectedItemsPaths = [];

            try
            {
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return selectedItemsPaths;
            }
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
                if (window.HWND == (int)handle)
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
            InternetExplorer? seekingWindow = null;
            int pathLength = 0;
            
            foreach (InternetExplorer window in shellWindows)
            {
                if (window.HWND != handle.ToInt32())
                    continue;

                if (window.LocationURL is null or [] || !Uri.TryCreate(window.LocationURL, UriKind.RelativeOrAbsolute, out var currentExplorerUri))
                    continue;

                string currentExplorerPath = currentExplorerUri.LocalPath;

                // The explorer window in Windows 11 supports multiple tabs, these tabs will have the same window handle, so we need to compare 
                // the title (indicates the path) of the current explorer window with the local path of each tab of this window. In recent Windows 11
                // versions, the Explorer title format has changed to "C:\\folder - File Explorer" or "C:\\folder and X more tabs - File Explorer".
                // So now it's unable to get current explorer window if it's Desktop/Downloads/Pictures etc
                if (windowTitle.StartsWith(currentExplorerPath) && currentExplorerPath.Length > pathLength)
                {
                    seekingWindow = window;
                    pathLength = currentExplorerPath.Length;
                }
            }

            return seekingWindow;
        }
    }
}
