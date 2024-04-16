using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WinTool.CommandLine;

namespace WinTool.Utils
{
    internal class ProcessHelper
    {
        private static readonly bool _isAdmin;

        public static string ProcessPath { get; }

        static ProcessHelper()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            _isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            ProcessPath = Path.Combine(Directory.GetCurrentDirectory(), "WinTool.exe");
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
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                Process.Start(psi);
                App.Current.Shutdown();
            }
        }
    }
}
