#pragma warning disable WPF0001
using GlobalKeyInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Native;
using WinTool.Options;
using WinTool.Services;
using WinTool.ViewModels;
using WinTool.ViewModels.Features;
using WinTool.ViewModels.Settings;
using WinTool.ViewModels.Shortcuts;
using WinTool.Views;
using WinTool.Views.Features;
using WinTool.Views.Settings;
using WinTool.Views.Shortcuts;

namespace WinTool;

public partial class App : Application
{
    private readonly IHost _app;
    private readonly ILogger _logger;

    private string? _currentLanguage;
    private AppTheme _currentTheme;
    private InputPopupWindow? _inputPopupWindow;
    private MainWindow? _mainWindow;

    public static CultureInfo SystemUICulture { get; } = Thread.CurrentThread.CurrentUICulture;
    public static CultureInfo SystemCulture { get; } = Thread.CurrentThread.CurrentCulture;

    public App()
    {
        CheckForSecondInstance();

        var builder = Host.CreateApplicationBuilder();

        builder.AddConfigurationFileProvider();
        builder.AddNLogConfiguration();

        builder.Services.Configure<SettingsOptions>(builder.Configuration.GetSection(nameof(SettingsOptions)));
        builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(nameof(FeaturesOptions)));
        builder.Services.Configure<ShortcutsOptions>(builder.Configuration.GetSection(nameof(ShortcutsOptions)));

        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<ShortcutsView>();
        builder.Services.AddTransient<FeaturesView>();
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddTransient<CreateFileWindow>();
        builder.Services.AddTransient<RunWithArgsWindow>();
        builder.Services.AddTransient<EditShortcutWindow>();
        builder.Services.AddTransient<InputPopupWindow>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<InputPopupViewModel>();
        builder.Services.AddTransient<ShortcutsViewModel>();
        builder.Services.AddTransient<FeaturesViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<CreateFileViewModel>();
        builder.Services.AddTransient<RunWithArgsViewModel>();
        builder.Services.AddTransient<EditShortcutViewModel>();
        builder.Services.AddSingleton<ShellCommandHandler>();
        builder.Services.AddSingleton<Shell>();
        builder.Services.AddSingleton<KeyboardLayoutManager>();
        builder.Services.AddSingleton<StaThreadService>();
        builder.Services.AddSingleton<ShortcutsService>();
        builder.Services.AddSingleton<ViewFactory>();
        builder.Services.AddSingleton<ShortcutContext>();
        builder.Services.AddSingleton<CreateFileDialogState>();
        builder.Services.AddSingleton<RunWithArgsDialogState>();
        builder.Services.AddSingleton<IKeyInterceptor>(new KeyInterceptor());
        builder.Services.AddSingleton<WritableOptions<SettingsOptions>>();
        builder.Services.AddSingleton<WritableOptions<FeaturesOptions>>();
        builder.Services.AddSingleton<WritableOptions<ShortcutsOptions>>();
        builder.Services.AddSingleton<IPostConfigureOptions<ShortcutsOptions>, PostConfigureShortcutsOptions>();
        builder.Services.AddSingleton<IPostConfigureOptions<SettingsOptions>, PostConfigureSettingsOptions>();

        builder.Services.AddHostedService(sp => sp.GetRequiredService<ShortcutsService>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<KeyboardLayoutManager>());

        _app = builder.Build();
        _logger = _app.Services.GetRequiredService<ILogger<App>>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _app.StartAsync();
        _logger.LogInformation("Application started with arguments: {Arguments}", string.Join(' ', e.Args));

        var clp = CommandLineParameters.Parse(e.Args);

        var settingsMonitor = _app.Services.GetRequiredService<IOptionsMonitor<SettingsOptions>>();
        settingsMonitor.OnChange(OnSettingsChanged);

        var settings = settingsMonitor.CurrentValue;
        ApplyLanguage(settings.Language);
        ApplyTheme(settings.AppTheme);
        RunAsAdminIfNeeded(settings, clp);

        // activate the popup window
        _inputPopupWindow = _app.Services.GetRequiredService<InputPopupWindow>();
        _mainWindow = _app.Services.GetRequiredService<MainWindow>();

        if (clp.BackgroundParameter is null)
            _mainWindow.Show();

        var shellCommandHandler = _app.Services.GetRequiredService<ShellCommandHandler>();
        HandleOperations(shellCommandHandler, clp);

        if (clp.ShutdownOnEndedParameter is not null)
            App.Current.Shutdown();

        base.OnStartup(e);
    }

    private void CheckForSecondInstance()
    {
        if (!Mutex.TryAttachAsFirstInstance() && NativeMethods.ShowWindow("WinTool"))
            App.Current.Shutdown();
    }

    private void RunAsAdminIfNeeded(SettingsOptions settings, CommandLineParameters clp)
    {
        if (settings.AlwaysRunAsAdmin && !Process.IsAdmin)
            Process.RestartAsAdmin(clp.ToString());
    }

    private void HandleOperations(ShellCommandHandler commandHandler, CommandLineParameters clp)
    {
        if (clp.CreateFileParameter is { FilePath: not (null or [])} createFile)
        {
            try
            {
                commandHandler.CreateFile(createFile.FilePath, createFile.Size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file {FilePath}", createFile.FilePath);
                MessageBox.ShowError(string.Format(WinTool.Properties.Resources.FileCreationError, createFile.FilePath, ex.Message));
            }
        }
    }

    private void ApplyLanguage(string? cultureName)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName ?? SystemUICulture.Name);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            WinTool.Properties.Resources.Culture = culture;

            _currentLanguage = culture.TwoLetterISOLanguageName;
        }
        catch
        {
            Thread.CurrentThread.CurrentUICulture = SystemUICulture;
            Thread.CurrentThread.CurrentCulture = SystemCulture;
            WinTool.Properties.Resources.Culture = null;

            _currentLanguage = SystemUICulture.TwoLetterISOLanguageName;
        }
    }

    private void ApplyTheme(AppTheme selectedTheme)
    {
        var selectedThemeName = selectedTheme.ToString();

        if (Current.ThemeMode.Value != selectedThemeName)
            Current.ThemeMode = new ThemeMode(selectedThemeName);

        var actualTheme = selectedTheme == AppTheme.System ? GetSystemTheme() : selectedTheme;

        if (actualTheme == AppTheme.Dark)
        {
            Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
            Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00));
        }
        else
        {
            Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        }

        _currentTheme = selectedTheme;
    }

    private AppTheme GetSystemTheme()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);

        if (key == null)
            return AppTheme.Light;

        var value = key.GetValue("AppsUseLightTheme");
        return value is int i && i == 0 ? AppTheme.Dark : AppTheme.Light;
    }

    private void RecreateMainWindow()
    {
        var wasVisible = _mainWindow?.IsVisible == true;
        _mainWindow?.ForceClose();
        _mainWindow = _app.Services.GetRequiredService<MainWindow>();

        if (wasVisible)
            _mainWindow.Show();
    }

    private void OnSettingsChanged(SettingsOptions settings, string? _)
    {
        Current.Dispatcher.BeginInvoke(() =>
        {
            if (settings.AppTheme != _currentTheme)
            {
                ApplyTheme(settings.AppTheme);
            }

            if (settings.Language != _currentLanguage)
            {
                ApplyLanguage(settings.Language);
                RecreateMainWindow();
            }
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Mutex.Release();
        _logger.LogInformation("Application is shutting down");

        await _app.StopAsync();
        base.OnExit(e);
    }
}
