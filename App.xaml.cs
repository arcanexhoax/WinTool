using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace WinTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string RegKeyName = "WinTool";

        private readonly Dictionary<string, string> _args = new();

        public App()
        {
            try
            {
                RegistryKey? runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string? exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (runKey is null || string.IsNullOrEmpty(exeDir))
                    return;

                // use arg "-background 1" to start app in background mode
                string factExePath = $"{Path.Combine(exeDir, "WinTool.exe")} -background 1";
                string? regExePath = runKey.GetValue(RegKeyName) as string;

                if (factExePath != regExePath)
                    runKey.SetValue(RegKeyName, factExePath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            MainWindow window = new();
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 1; i < args.Length; i += 2)
            {
                string arg = args[i].Replace("-", string.Empty);
                _args.Add(arg, args[i + 1]);
            }

            if (_args.TryGetValue("background", out string? value) && value == "1")
                return;

            window.Show();
        }
    }
}
