using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using WinTool.WinUI.CommandLine;
using WinTool.WinUI.Extensions;
using WinTool.WinUI.Models;
using WinTool.WinUI.Native;
using WinTool.WinUI.Options;
using WinTool.WinUI.Services;
using WinTool.WinUI.ViewModels;
using WinTool.WinUI.ViewModels.Features;
using WinTool.WinUI.ViewModels.Settings;
using WinTool.WinUI.ViewModels.Shortcuts;
using WinTool.WinUI.Views;

namespace WinTool.WinUI;

public partial class App : Application
{
    private readonly IHost _app;
    private readonly ILogger _logger;

    private static Action s_shutdown = null!;

    private bool _isUpdateAvailable;
    private bool _isShuttingDown;
    private string? _currentLanguage;
    private AppTheme _currentTheme;
    //private InputPopupWindow? _inputPopupWindow;
    private TaskbarIcon? _trayIcon;
    private UpdateService? _updateService;
    private IOptionsMonitor<SettingsOptions>? _settingsOptions;

    public static MainWindow? MainWindow { get; private set; }
    public static CultureInfo SystemUICulture { get; } = CultureInfo.CurrentUICulture;
    public static CultureInfo SystemCulture { get; } = Thread.CurrentThread.CurrentCulture;
    public static DispatcherQueue Dispatcher { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        Dispatcher = DispatcherQueue.GetForCurrentThread();
        s_shutdown = ShutdownAsync;

        var builder = Host.CreateApplicationBuilder();

        builder.AddConfigurationFileProvider();
        builder.AddNLogConfiguration();

        builder.Services.Configure<SettingsOptions>(builder.Configuration.GetSection(nameof(SettingsOptions)));
        builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(nameof(FeaturesOptions)));
        builder.Services.Configure<ShortcutsOptions>(builder.Configuration.GetSection(nameof(ShortcutsOptions)));
        builder.Services.Configure<UpdateOptions>(builder.Configuration.GetSection(nameof(UpdateOptions)));

        builder.Services.AddTransient<MainWindow>();
        //builder.Services.AddTransient<ShortcutsView>();
        //builder.Services.AddTransient<EditShortcutView>();
        //builder.Services.AddTransient<FeaturesView>();
        //builder.Services.AddTransient<SettingsView>();
        //builder.Services.AddTransient<RunWithArgsWindow>();
        //builder.Services.AddTransient<InputPopupWindow>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<InputPopupViewModel>();
        builder.Services.AddTransient<ShortcutsViewModel>();
        builder.Services.AddTransient<FeaturesViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<RunWithArgsViewModel>();
        builder.Services.AddTransient<EditShortcutViewModel>();
        builder.Services.AddSingleton<ShellCommandHandler>();
        builder.Services.AddSingleton<Shell>();
        builder.Services.AddSingleton<KeyboardLayoutManager>();
        builder.Services.AddSingleton<StaThreadService>();
        builder.Services.AddSingleton<ShortcutsService>();
        builder.Services.AddSingleton<UpdateService>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ProcessHelper>();
        builder.Services.AddSingleton<DialogService>();
        builder.Services.AddSingleton<ShortcutContext>();
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<RunWithArgsDialogState>();
        builder.Services.AddSingleton<IKeyInterceptor>(_ => new KeyInterceptor());
        builder.Services.AddSingleton<WritableOptions<SettingsOptions>>();
        builder.Services.AddSingleton<WritableOptions<FeaturesOptions>>();
        builder.Services.AddSingleton<WritableOptions<ShortcutsOptions>>();
        builder.Services.AddSingleton<WritableOptions<UpdateOptions>>();
        builder.Services.AddSingleton<IPostConfigureOptions<ShortcutsOptions>, PostConfigureShortcutsOptions>();
        builder.Services.AddSingleton<IPostConfigureOptions<SettingsOptions>, PostConfigureSettingsOptions>();

        builder.Services.AddHostedService(sp => sp.GetRequiredService<ShortcutsService>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<KeyboardLayoutManager>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<UpdateService>());

        _app = builder.Build();
        _logger = _app.Services.GetRequiredService<ILogger<App>>();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (CheckForSecondInstance())
            return;

        await _app.StartAsync();
        _logger.LogInformation("Application started with arguments: {Arguments}", args.Arguments);

        var clp = CommandLineParameters.Parse(Environment.GetCommandLineArgs()[1..]);

        _settingsOptions = _app.Services.GetRequiredService<IOptionsMonitor<SettingsOptions>>();
        _settingsOptions.OnChange(OnSettingsChanged);

        var settings = _settingsOptions.CurrentValue;
        var processHelper = _app.Services.GetRequiredService<ProcessHelper>();

        ApplyLanguage(settings.Language);
        ApplyTheme(settings.AppTheme);

        if (RunAsAdminIfNeeded(processHelper, settings, clp))
            return;

        // activate the popup window
        //_inputPopupWindow = _app.Services.GetRequiredService<InputPopupWindow>();
        MainWindow = _app.Services.GetRequiredService<MainWindow>();

        InitializeTrayIcon();

        _updateService = _app.Services.GetRequiredService<UpdateService>();
        _updateService.UpdateChecked += OnUpdateChecked;
        _updateService.NewUpdateAvailable += OnNewUpdateAvailable;
        ApplyUpdateAvailability(_updateService.CurrentBackgroundCheckState.State == UpdateState.Available);

        if (clp.BackgroundParameter is null)
            OpenMainWindow();

        var shellCommandHandler = _app.Services.GetRequiredService<ShellCommandHandler>();
        HandleOperations(shellCommandHandler, clp);

        if (clp.ShutdownOnEndedParameter is not null)
            Shutdown();
    }

    private bool CheckForSecondInstance()
    {
        if (Mutex.TryAttachAsFirstInstance())
            return false;

        NativeMethods.ShowWindow("WinTool");
        Shutdown();
        return true;
    }

    private bool RunAsAdminIfNeeded(ProcessHelper processHelper, SettingsOptions settings, CommandLineParameters clp)
    {
        if (settings.AlwaysRunAsAdmin && !processHelper.IsAdmin)
        {
            processHelper.RestartAsAdmin(clp.ToString());
            return true;
        }

        return false;
    }

    private void HandleOperations(ShellCommandHandler commandHandler, CommandLineParameters clp)
    {
        if (clp.CreateFileParameter?.FilePath is { Length: > 0 } filePath)
        {
            try
            {
                commandHandler.CreateFile(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file {FilePath}", filePath);
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
            Properties.Resources.Culture = culture;

            _currentLanguage = culture.TwoLetterISOLanguageName;
        }
        catch
        {
            Thread.CurrentThread.CurrentUICulture = SystemUICulture;
            Thread.CurrentThread.CurrentCulture = SystemCulture;
            Properties.Resources.Culture = null;

            _currentLanguage = SystemUICulture.TwoLetterISOLanguageName;
        }
    }

    private void ApplyTheme(AppTheme selectedTheme)
    {
        var selectedThemeName = selectedTheme.ToString();

        //if (Current.ThemeMode.Value != selectedThemeName)
        //    Current.ThemeMode = new ThemeMode(selectedThemeName);

        //var actualTheme = selectedTheme == AppTheme.System ? GetSystemTheme() : selectedTheme;

        //if (actualTheme == AppTheme.Dark)
        //{
        //    Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
        //    Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00));
        //}
        //else
        //{
        //    Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
        //    Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        //}

        _currentTheme = selectedTheme;
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = (TaskbarIcon)Resources["TrayIcon"];
        _trayIcon.LeftClickCommand = new RelayCommand(OnTrayIconOpen);

        var menu = (MenuFlyout)_trayIcon.ContextFlyout;
        ((MenuFlyoutItem)menu.Items[0]).Command = new RelayCommand(OnTrayIconOpen);
        ((MenuFlyoutItem)menu.Items[2]).Command = new RelayCommand(OnTrayIconClose);

        UpdateTrayIconLocalization();
        ApplyUpdateAvailability(_isUpdateAvailable);

        _trayIcon.Visibility = Visibility.Visible;
        _trayIcon.ForceCreate(enablesEfficiencyMode: false);
    }

    private void UpdateTrayIconLocalization()
    {
        if (_trayIcon is null)
            return;

        // TODO use .resw to remove this code
        _trayIcon.ToolTipText = Properties.Resources.TrayToolTip;

        var menu = (MenuFlyout)_trayIcon.ContextFlyout;
        ((MenuFlyoutItem)menu.Items[0]).Text = Properties.Resources.Open;
        ((MenuFlyoutItem)menu.Items[2]).Text = Properties.Resources.Close;
    }

    private void ApplyUpdateAvailability(bool isUpdateAvailable)
    {
        _isUpdateAvailable = isUpdateAvailable;
        MainWindow?.SetUpdateOverlay(isUpdateAvailable);

        if (_trayIcon is null)
            return;

        var iconName = isUpdateAvailable ? "update.ico" : "icon.ico";
        _trayIcon.Icon = new System.Drawing.Icon(Path.Combine(AppContext.BaseDirectory, "Resources", iconName));
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

    private void OpenMainWindow()
    {
        _app.Services.GetRequiredService<AppState>().IsBackgroundMode = false;

        if (MainWindow is null)
            return;

        if (!MainWindow.AppWindow.IsVisible)
            MainWindow.AppWindow.Show();

        if (MainWindow.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized } presenter)
            presenter.Restore();

        MainWindow.Activate();
    }

    private void RecreateMainWindow()
    {
        var wasVisible = MainWindow?.AppWindow.IsVisible == true;
        MainWindow?.ForceClose();
        MainWindow = _app.Services.GetRequiredService<MainWindow>();

        ApplyUpdateAvailability(_isUpdateAvailable);

        if (wasVisible)
            OpenMainWindow();
    }

    private void OnTrayIconOpen() => OpenMainWindow();

    private void OnTrayIconClose() => Shutdown();

    private void OnUpdateNotificationClicked(object sender, RoutedEventArgs e)
    {
        OpenMainWindow();
        //_mainWindow?.OpenAboutSettings();
    }

    private void OnNewUpdateAvailable(UpdateCheckResult result)
    {
        Dispatcher.TryEnqueue(() =>
        {
            if (_settingsOptions?.CurrentValue.Notifications.NewVersions == false)
                return;

            //_trayIcon?.ShowBalloonTip(
            //    WinTool.Properties.Resources.WinTool,
            //    $"{WinTool.Properties.Resources.NewVersionAvailable}: {result.LatestVersion.ToString(3)}",
            //    BalloonIcon.Info);
        });
    }

    private void OnUpdateChecked(UpdateCheckResult result)
    {
        Dispatcher.TryEnqueue(() => ApplyUpdateAvailability(result.IsUpdateAvailable));
    }

    private void OnSettingsChanged(SettingsOptions settings, string? _)
    {
        Dispatcher.TryEnqueue(() =>
        {
            if (settings.AppTheme != _currentTheme)
            {
                ApplyTheme(settings.AppTheme);
                //_inputPopupWindow?.RefreshBackdrop();
            }

            if (settings.Language != _currentLanguage)
            {
                ApplyLanguage(settings.Language);
                UpdateTrayIconLocalization();
                RecreateMainWindow();
            }
        });
    }

    public static void Shutdown() => Dispatcher.TryEnqueue(() => s_shutdown());

    private async void ShutdownAsync()
    {
        if (_isShuttingDown)
            return;

        _isShuttingDown = true;
        _updateService?.UpdateChecked -= OnUpdateChecked;
        _updateService?.NewUpdateAvailable -= OnNewUpdateAvailable;
        _trayIcon?.Dispose();

        await _app.StopAsync();
        _app.Dispose();
        Mutex.Release();

        _logger.LogInformation("Application is shutting down");
        Exit();
    }
}
