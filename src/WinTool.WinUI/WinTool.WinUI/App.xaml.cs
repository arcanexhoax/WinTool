using GlobalKeyInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Native;
using WinTool.Options;
using WinTool.Services;
using WinTool.Utils;
using WinTool.ViewModels;
using WinTool.ViewModels.Features;
using WinTool.ViewModels.Settings;
using WinTool.ViewModels.Shortcuts;
using WinTool.Views;
using WinTool.Views.Features;
using WinTool.Views.Settings;
using WinTool.Views.Shortcuts;

namespace WinTool.WinUI;

public partial class App : Application
{
    private readonly IHost _app;

    private Mutex? _secondInstanceMutex;

    public App()
    {
        DispatcherQueue.GetForCurrentThread().ShutdownStarting += OnShutdownStarting;

        InitializeComponent();
        CheckForSecondInstance();

        var builder = Host.CreateApplicationBuilder();

        builder.AddConfigurationFileProvider();

        builder.Services.Configure<SettingsOptions>(builder.Configuration.GetSection(nameof(SettingsOptions)));
        builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(nameof(FeaturesOptions)));
        builder.Services.Configure<ShortcutsOptions>(builder.Configuration.GetSection(nameof(ShortcutsOptions)));

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<ShortcutsView>();
        builder.Services.AddSingleton<FeaturesView>();
        builder.Services.AddSingleton<SettingsView>();
        builder.Services.AddTransient<CreateFileWindow>();
        builder.Services.AddTransient<RunWithArgsWindow>();
        builder.Services.AddTransient<EditShortcutWindow>();
        // TODO add
        //builder.Services.AddSingleton<SwitchLanguageWindow>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<SwitchLanguageViewModel>();
        builder.Services.AddSingleton<ShortcutsViewModel>();
        builder.Services.AddSingleton<FeaturesViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<CreateFileViewModel>();
        builder.Services.AddSingleton<RunWithArgsViewModel>();
        builder.Services.AddSingleton<EditShortcutViewModel>();
        builder.Services.AddSingleton<ShellCommandHandler>();
        builder.Services.AddSingleton<Shell>();
        builder.Services.AddSingleton<KeyboardLayoutManager>();
        builder.Services.AddSingleton<StaThreadService>();
        builder.Services.AddSingleton<ViewFactory>();
        builder.Services.AddSingleton<WritableOptions<SettingsOptions>>();
        builder.Services.AddSingleton<WritableOptions<FeaturesOptions>>();
        builder.Services.AddSingleton<WritableOptions<ShortcutsOptions>>();
        builder.Services.AddSingleton(new KeyInterceptor());
        builder.Services.AddSingleton<ShortcutContext>();
        builder.Services.AddSingleton<ShortcutsService>();
        builder.Services.AddSingleton<IPostConfigureOptions<ShortcutsOptions>, PostConfigureShortcutsOptions>();

        builder.Services.AddHostedService(sp => sp.GetRequiredService<ShortcutsService>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<KeyboardLayoutManager>());

        _app = builder.Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _app.StartAsync();

        var clp = CommandLineParameters.Parse(Environment.GetCommandLineArgs());
        var settings = _app.Services.GetRequiredService<IOptions<SettingsOptions>>().Value;

        RestartAsAdminIfNeeded(settings, clp);

        // TODO add
        // activate the popup window
        //_app.Services.GetRequiredService<SwitchLanguageWindow>();
        var mainWindow = _app.Services.GetRequiredService<MainWindow>();
        var commandHandler = _app.Services.GetRequiredService<ShellCommandHandler>();

        if (clp.BackgroundParameter is null)
            mainWindow.Activate();

        HandleOperations(commandHandler, clp);

        if (clp.ShutdownOnEndedParameter is not null)
            App.Current.Exit();
    }

    private void CheckForSecondInstance()
    {
        _secondInstanceMutex = new Mutex(true, "WinTool-10fdf33711f4591a368bd6a0b0e20cc1", out bool isFirstInstance);

        if (!isFirstInstance && NativeMethods.ShowWindow("WinTool"))
            Environment.Exit(0);
    }

    private void RestartAsAdminIfNeeded(SettingsOptions settings, CommandLineParameters clp)
    {
        if (settings.AlwaysRunAsAdmin && !ProcessHelper.IsAdmin)
            ProcessHelper.RestartAsAdmin(clp);
    }

    private void HandleOperations(ShellCommandHandler commandHandler, CommandLineParameters clp)
    {
        if (clp.CreateFileParameter is { FilePath: not (null or []) } createFile)
        {
            try
            {
                commandHandler.CreateFile(createFile.FilePath, createFile.Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating file {createFile.FilePath}: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Properties.Resources.FileCreationError, createFile.FilePath, ex.Message));
            }
        }
    }

    private async void OnShutdownStarting(DispatcherQueue sender, DispatcherQueueShutdownStartingEventArgs args)
    {
        _secondInstanceMutex?.Dispose();
        await _app.StopAsync();
    }
}
