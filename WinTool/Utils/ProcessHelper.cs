using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using WinTool.CommandLine;
using WinTool.Properties;

namespace WinTool.Utils;

internal class ProcessHelper
{
    private static readonly bool _isAdmin;
    private static readonly string _appDirectory;

    public static string ProcessPath { get; }

    static ProcessHelper()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        _isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        _appDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
        ProcessPath = Path.Combine(_appDirectory, "WinTool.exe");
    }

    public static void ExecuteWithUacIfNeeded(Action action, CommandLineParameters clp)
    {
        try
        {
            action?.Invoke();
        }
        catch (UnauthorizedAccessException ex)
        {
            if (_isAdmin)
            {
                Debug.WriteLine($"Already running as admin but got: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Resources.AlreadyRunnningAsAdminError, ex.Message));
                return;
            }

            ProcessStartInfo psi = new()
            {
                Arguments = clp.ToString(),
                FileName = ProcessPath,
                Verb = "runas",
                UseShellExecute = true,
                WorkingDirectory = _appDirectory
            };

            try
            {
                Process.Start(psi);
                App.Current.Shutdown();
            }
            catch (Exception iex)
            {
                Debug.WriteLine("Failed to start process with UAC: " + iex.Message);
                MessageBoxHelper.ShowError(string.Format(Resources.RunAsAdminError, iex.Message));
            }
        }
    }
}
