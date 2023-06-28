using System;
using System.Windows;
using WinTool.Model;

namespace WinTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow window = new();
            string[] args = Environment.GetCommandLineArgs();

            var clp = CommandLineParameters.Parse(args);

            if (!clp.Background)
                window.Show();
        }
    }
}
