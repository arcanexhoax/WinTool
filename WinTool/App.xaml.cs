using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Native;
using WinTool.Services;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;

namespace WinTool
{
    public partial class App : Application
    {
        private readonly IHost _app;

        private Mutex? _secondInstanceMutex;

        public App()
        {
            CheckForSecondInstance();

            var builder = Host.CreateApplicationBuilder();

            builder.Services.AddSingleton<MainWindow>();
            builder.Services.AddSingleton<SwitchLanguageWindow>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CommandHandler>();
            builder.Services.AddSingleton<LanguagePopupHandler>();
            builder.Services.AddSingleton<Shell>();
            builder.Services.AddSingleton<SettingsManager>();
            builder.Services.AddSingleton<MemoryCache>();

            _app = builder.Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _app.StartAsync();

            var mainWindow = _app.Services.GetRequiredService<MainWindow>();
            var commandHandler = _app.Services.GetRequiredService<CommandHandler>();

            var clp = CommandLineParameters.Parse(e.Args);

            if (clp.BackgroundParameter is null)
                mainWindow.Show();

            HandleOperations(commandHandler, clp);

            if (clp.ShutdownOnEndedParameter is not null)
                App.Current.Shutdown();

            base.OnStartup(e);

            var languagePopupHandler = _app.Services.GetRequiredService<LanguagePopupHandler>();
            await languagePopupHandler.Start();
        }

        private void CheckForSecondInstance()
        {
            _secondInstanceMutex = new Mutex(true, "WinTool-10fdf33711f4591a368bd6a0b0e20cc1", out bool isFirstInstance);

            if (!isFirstInstance && NativeMethods.ShowWindow("WinTool"))
                Environment.Exit(0);
        }

        private void HandleOperations(CommandHandler commandHandler, CommandLineParameters clp)
        {
            if (clp.CreateFileParameter is { FilePath: not (null or [])})
            {
                try
                {
                    commandHandler.CreateFile(clp.CreateFileParameter.FilePath, clp.CreateFileParameter.Size);
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.ShowError(ex.Message);
                }
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _secondInstanceMutex?.Dispose();
            await _app.StopAsync();
            base.OnExit(e);
        }
    }
}
