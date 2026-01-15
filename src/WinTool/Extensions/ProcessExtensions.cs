using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using WinTool.Properties;

namespace WinTool.Extensions;

public static class ProcessExtensions
{
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
                Debug.WriteLine($"Already running as admin but got: {ex.Message}");
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
                Debug.WriteLine("Failed to start process with UAC: " + iex.Message);
                MessageBox.ShowError(string.Format(Resources.RunAsAdminError, iex.Message));
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
}
