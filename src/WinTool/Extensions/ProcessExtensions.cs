using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using WinTool.Properties;

namespace WinTool.Extensions;

public static class ProcessExtensions
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private static readonly bool s_isAdmin;

    static ProcessExtensions()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        s_isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    extension(Process)
    {
        public static bool IsAdmin => s_isAdmin;

        public static void ExecuteAsAdmin(Action action, string? args = null)
        {
            try
            {
                action?.Invoke();
            }
            catch (UnauthorizedAccessException ex) when (s_isAdmin)
            {
                s_logger.Error(ex, "Already running as admin but got an unauthorized access error");
                MessageBox.ShowError(string.Format(Resources.AlreadyRunnningAsAdminError, ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                RestartAsAdmin(args);
            }
        }

        public static void RestartAsAdmin(string? args = null)
        {
            try
            {
                Mutex.Release();
                Start(Environment.ProcessPath!, args, true);
                App.Current.Dispatcher.Invoke(App.Current.Shutdown);
            }
            catch (Exception iex)
            {
                s_logger.Error(iex, "Failed to start process with UAC");
                MessageBox.ShowError(string.Format(Resources.RunAsAdminError, iex.Message));
            }
        }

        public static void Start(string fileName, string? args, bool asAdmin)
        {
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

        private static void StartWithElevatedCmd(string fileName, string? args)
        {
            var psi = new ProcessStartInfo()
            {
                Arguments = $"/d /v:off /c start \"\" \"{fileName}\" {args}",
                FileName = "cmd.exe",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            Process.Start(psi);
        }
    }
}
