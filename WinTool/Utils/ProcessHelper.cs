using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using WinTool.CommandLine;

namespace WinTool.Utils
{
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

            _appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            ProcessPath = Path.Combine(_appDirectory, "WinTool.exe");
        }

        public static void ExecuteWithUacIfNeeded(Action action, ICommandLineParameter clp)
        {
            try
            {
                action?.Invoke();
            }
            catch (UnauthorizedAccessException)
            {
                if (_isAdmin)
                    throw;

                ProcessStartInfo psi = new()
                {
                    Arguments = clp.ToString(),
                    FileName = ProcessPath,
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = _appDirectory
                };

                Process.Start(psi);
                App.Current.Shutdown();
            }
        }
    }
}
