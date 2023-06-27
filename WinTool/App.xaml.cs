using System;
using System.Collections.Generic;
using System.Windows;

namespace WinTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Dictionary<string, string> _args = new();

        public App()
        {
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
