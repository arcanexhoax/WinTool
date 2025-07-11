using GlobalKeyInterceptor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Native;
using WinTool.Options;
using WinTool.Services;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;

namespace WinTool;

public partial class App : Application
{
    private readonly IHost _app;

    private Mutex? _secondInstanceMutex;

    public App()
    {
        CheckForSecondInstance();

        var builder = Host.CreateApplicationBuilder();

        builder.AddConfigurationFileProvider();

        builder.Services.Configure<SettingsOptions>(builder.Configuration.GetSection(nameof(SettingsOptions)));

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<SwitchLanguageWindow>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<SwitchLanguageViewModel>();
        builder.Services.AddSingleton<ShortcutsViewModel>();
        builder.Services.AddSingleton<FeaturesViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<CommandHandler>();
        builder.Services.AddSingleton<KeyInterceptor>();
        builder.Services.AddSingleton<Shell>();
        builder.Services.AddSingleton<SettingsManager>();
        builder.Services.AddSingleton<KeyboardLayoutManager>();
        builder.Services.AddSingleton<MemoryCache>();
        builder.Services.AddSingleton<WritableOptions<SettingsOptions>>();

        _app = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _app.StartAsync();

        // activate the window
        _app.Services.GetRequiredService<SwitchLanguageWindow>();
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

    private void CheckForSecondInstance()
    {
        _secondInstanceMutex = new Mutex(true, "WinTool-10fdf33711f4591a368bd6a0b0e20cc1", out bool isFirstInstance);

        if (!isFirstInstance && NativeMethods.ShowWindow("WinTool"))
            Environment.Exit(0);
    }

    private void HandleOperations(CommandHandler commandHandler, CommandLineParameters clp)
    {
        if (clp.CreateFileParameter is { FilePath: not (null or [])} createFile)
        {
            try
            {
                commandHandler.CreateFile(createFile.FilePath, createFile.Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating file {createFile.FilePath}: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(WinTool.Properties.Resources.FileCreationError, createFile.FilePath, ex.Message));
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
