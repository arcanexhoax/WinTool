using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using WinTool.Extensions;
using WinTool.Properties;

namespace WinTool.Services;

public class ProcessHelper
{
    private readonly ILogger _logger;
    private readonly Shell _shell;

    public bool IsAdmin { get; }

    public ProcessHelper(ILogger<ProcessHelper> logger, Shell shell)
    {
        _logger = logger;
        _shell = shell;

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void ExecuteAsAdmin(Action action, string? args = null)
    {
        try
        {
            action?.Invoke();
        }
        catch (UnauthorizedAccessException ex) when (IsAdmin)
        {
            _logger.LogError(ex, "Already running as admin but got an unauthorized access error");
            MessageBox.ShowError(string.Format(Resources.AlreadyRunnningAsAdminError, ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            RestartAsAdmin(args);
        }
    }

    public void RestartAsAdmin(string? args = null)
    {
        try
        {
            Mutex.Release();
            Start(Environment.ProcessPath!, args, true);
            App.Current.Dispatcher.Invoke(App.Current.Shutdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start process with UAC");
            MessageBox.ShowError(string.Format(Resources.RunAsAdminError, ex.Message));
        }
    }

    public void Start(string fileName, string? args, bool asAdmin)
    {
        if (!asAdmin && IsAdmin)
        {
            _shell.StartProcessUnelevated(fileName, args);
            return;
        }

        var psi = new ProcessStartInfo()
        {
            Arguments = args ?? string.Empty,
            FileName = fileName,
            Verb = asAdmin ? "runas" : string.Empty,
            UseShellExecute = true,
        };

        try
        {
            Process.Start(psi);
        }
        catch (Win32Exception ex) when (asAdmin && ex.NativeErrorCode == 1155)
        {
            StartWithElevatedCmd(fileName, args);
        }
    }

    private void StartWithElevatedCmd(string fileName, string? args)
    {
        var escapedArgs = EscapeCmdCommandLine(args);
        var psi = new ProcessStartInfo()
        {
            Arguments = $"/d /v:off /c start \"\" \"{fileName}\" {escapedArgs}",
            FileName = "cmd.exe",
            Verb = "runas",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        Process.Start(psi);
    }

    private static string EscapeCmdCommandLine(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return string.Empty;

        var escaped = new StringBuilder(commandLine.Length);
        var isQuoted = false;

        foreach (char character in commandLine)
        {
            if (character == '"')
                isQuoted = !isQuoted;
            else if (!isQuoted && character is '^' or '&' or '|' or '<' or '>' or '%')
                escaped.Append('^');

            escaped.Append(character);
        }

        return escaped.ToString();
    }
}
