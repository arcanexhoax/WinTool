using SHDocVw;
using System;
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

        private static string? GetActiveExplorerPath()
        {
            try
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();
                ShellWindows shellWindows = new();

                foreach (InternetExplorer window in shellWindows)
                {
                    if (window.HWND != (int)handle)
                        continue;

                    var shellWindow = window.Document as Shell32.IShellFolderViewDual2;

                    if (shellWindow == null)
                        continue;

                    var currentFolder = shellWindow.Folder.Items().Item();

                    // special folder - use window title, for some reason on "Desktop" gives null
                    if (currentFolder == null || currentFolder.Path.StartsWith("::"))
                    {
                        const int nChars = 256;
                        StringBuilder buff = new(nChars);
                        if (NativeMethods.GetWindowText(handle, buff, nChars) > 0)
                        {
                            return buff.ToString();
                        }
                    }
                    else
                        return currentFolder.Path;

                    break;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return null;
        }
    }
}
