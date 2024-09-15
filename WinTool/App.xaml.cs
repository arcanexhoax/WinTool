using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Modules;
using WinTool.ViewModel;

namespace WinTool
{
    public partial class App : Application
    {
        private readonly IHost _app;

        public App()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<CommandHandler>();
                services.AddSingleton<Shell>();
                services.AddSingleton<SettingsManager>();
                services.AddSingleton<MemoryCache>();
            });

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
        }

        private void HandleOperations(CommandHandler commandHandler, CommandLineParameters clp)
        {
            if (clp.CreateFileParameter is { FilePath: not (null or [])})
            {
                commandHandler.CreateFile(clp.CreateFileParameter.FilePath, clp.CreateFileParameter.Size);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _app.StopAsync();
            base.OnExit(e);
        }
    }
}
