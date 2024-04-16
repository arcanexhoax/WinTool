using System;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Modules;

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

            if (clp.BackgroundParameter is null)
                window.Show();

            HandleOperations(clp);

            if (clp.ShutdownOnEndedParameter is not null)
                App.Current.Shutdown();
        }

        private void HandleOperations(CommandLineParameters clp)
        {
            if (clp.CreateFileParameter is { FilePath: not (null or [])})
            {
                CommandHandler.CreateFile(clp.CreateFileParameter.FilePath, clp.CreateFileParameter.Size);
            }
        }
    }
}
