using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;

namespace WinTool.Modules
{
    internal class Shell
    {
        public static Task<string?> GetActiveExplorerPathAsync()
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

        public static Task<List<string>> GetSelectedItemsPathsAsync()
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

        private static string? GetActiveExplorerPath()
        {
            try
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();
                InternetExplorer? window = GetActiveShellWindow(handle);

                if (window is null)
                    return null;

                var shellWindow = window.Document as IShellFolderViewDual2;

                if (shellWindow == null)
                    return null;

                var currentFolder = shellWindow.Folder.Items().Item();

                // special folder - use window title, for some reason on "Desktop" gives null
                if (currentFolder == null || currentFolder.Path.StartsWith("::"))
                {
                    const int nChars = 256;
                    StringBuilder buff = new(nChars);

                    if (NativeMethods.GetWindowText(handle, buff, nChars) > 0)
                        return buff.ToString();

                    return null;
                }
                else
                    return currentFolder.Path;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        private static List<string> GetSelectedItemsPaths()
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

        private static InternetExplorer? GetActiveShellWindow(IntPtr handle)
        {
            ShellWindows shellWindows = new();

            foreach (InternetExplorer window in shellWindows)
            {
                if (window.HWND == (int)handle)
                    return window;
            }

            return null;
        }
    }
}
