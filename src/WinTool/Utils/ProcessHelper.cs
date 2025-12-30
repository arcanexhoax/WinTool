using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using WinTool.CommandLine;
using WinTool.Properties;

namespace WinTool.Utils;

internal class ProcessHelper
{
    public static bool IsAdmin { get; }
    public static string ProcessPath { get; }

    static ProcessHelper()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        ProcessPath = Environment.ProcessPath!;
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
        try
        {
            Start(ProcessPath, clp?.ToString(), true);
            App.Current.Shutdown();
        }
        catch (Exception iex)
        {
            Debug.WriteLine("Failed to start process with UAC: " + iex.Message);
            MessageBoxHelper.ShowError(string.Format(Resources.RunAsAdminError, iex.Message));
        }
    }

    public static void Start(string fileName, string? args, bool asAdmin = false)
    {
        var psi = new ProcessStartInfo()
        {
            Arguments = args ?? string.Empty,
            FileName = fileName,
            Verb = asAdmin ? "runas" : string.Empty,
            UseShellExecute = true,
        };

        Process.Start(psi);
    }
}
