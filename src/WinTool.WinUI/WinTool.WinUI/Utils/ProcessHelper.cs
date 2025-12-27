using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using WinTool.CommandLine;
using WinTool.Properties;

namespace WinTool.Utils;

internal class ProcessHelper
{
    private static readonly string s_appDirectory;

    public static bool IsAdmin { get; }
    public static string ProcessPath { get; }

    static ProcessHelper()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        s_appDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
        ProcessPath = Path.Combine(s_appDirectory, "WinTool.exe");
    }

    public static void ExecuteAsAdminIfNeeded(Action action, CommandLineParameters clp)
    {
        try
        {
            action?.Invoke();
        }
        catch (UnauthorizedAccessException ex) when (IsAdmin)
        {
            Debug.WriteLine($"Already running as admin but got: {ex.Message}");
            MessageBoxHelper.ShowError(string.Format(Resources.AlreadyRunnningAsAdminError, ex.Message));
        }
        catch (UnauthorizedAccessException) 
        {
            RestartAsAdmin(clp);
        }
    }

    public static void RestartAsAdmin(CommandLineParameters? clp = null)
    {
        var psi = new ProcessStartInfo()
        {
            Arguments = clp?.ToString(),
            FileName = ProcessPath,
            Verb = "runas",
            UseShellExecute = true,
            WorkingDirectory = s_appDirectory
        };

        try
        {
            Process.Start(psi);
            // TODO shutdown app
            //App.Current.Shutdown();
        }
        catch (Exception iex)
        {
            Debug.WriteLine("Failed to start process with UAC: " + iex.Message);
            MessageBoxHelper.ShowError(string.Format(Resources.RunAsAdminError, iex.Message));
        }
    }
}
