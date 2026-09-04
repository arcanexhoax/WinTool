using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;

namespace WinTool.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private const string GitHubUri = "https://github.com/arcanexhoax/WinTool";
    private const string RegKeyName = "WinTool";

    private readonly string _executionFilePath;
    private readonly ILogger _logger;
    private readonly ProcessHelper _processHelper;
    private readonly WritableOptions<SettingsOptions> _settingsOptions;
    private readonly UpdateService _updateService;
    private readonly Version _currentVersion = typeof(SettingsViewModel).Assembly.GetName().Version ?? new Version(0, 0, 0);

    private bool _isInitializing;
    private Uri? _releaseUri;
    private GitHubReleaseAsset? _updateAsset;
    private CancellationTokenSource? _downloadCts;

    public bool LaunchOnWindowsStartup
    {
        get; set
        {
            try
            {
                if (SetProperty(ref field, value) && !_isInitializing)
                {
                    _settingsOptions.Update(o => o.WindowsStartupEnabled = value);
                }

                using RegistryKey runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) ??
                    throw new InvalidOperationException("Unable to open registry.");

                if (value)
                {
                    runKey.SetValue(RegKeyName, _executionFilePath);
                }
                else
                {
                    if (runKey.GetValue(RegKeyName) is not null)
                        runKey.DeleteValue(RegKeyName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Windows startup");
                MessageBox.ShowError(string.Format(Resources.SetWindowsStartupError, ex.Message));
            }
        }
    }

    public bool AlwaysRunAsAdmin
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _settingsOptions.Update(o => o.AlwaysRunAsAdmin = value);
            }
        }
    }

    public AppTheme SelectedAppTheme
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _settingsOptions.Update(o => o.AppTheme = value);
            }
        }
    }

    public AnimationMode SelectedAnimationMode
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _settingsOptions.Update(o => o.AnimationMode = value);
            }
        }
    }

    public string? SelectedLanguage
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _settingsOptions.Update(o => o.Language = value);
            }
        }
    }

    [ObservableProperty]
    public partial UpdateState UpdateState { get; set; }

    [ObservableProperty]
    public partial string CurrentVersion { get; set; }

    [ObservableProperty]
    public partial string AvailableVersion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    [ObservableProperty]
    public partial string DownloadProgressText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpdateErrorMessage { get; set; } = string.Empty;

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        ProcessHelper processHelper,
        WritableOptions<SettingsOptions> settingsOptions,
        UpdateService updateService)
    {
        // use arg "/background" to start app in background mode
        _executionFilePath = $"\"{Environment.ProcessPath!}\" {BackgroundParameter.ParameterName}";
        _logger = logger;
        _processHelper = processHelper;
        _settingsOptions = settingsOptions;
        _updateService = updateService;
        _isInitializing = true;

        LaunchOnWindowsStartup = _settingsOptions.CurrentValue.WindowsStartupEnabled;
        AlwaysRunAsAdmin = _settingsOptions.CurrentValue.AlwaysRunAsAdmin;
        SelectedAppTheme = _settingsOptions.CurrentValue.AppTheme;
        SelectedAnimationMode = _settingsOptions.CurrentValue.AnimationMode;
        SelectedLanguage = _settingsOptions.CurrentValue.Language;
        CurrentVersion = _currentVersion.ToString(3);

        _isInitializing = false;
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        UpdateState = UpdateState.Checking;
        UpdateErrorMessage = string.Empty;
        _updateAsset = null;
        _releaseUri = null;

        try
        {
            var result = await _updateService.CheckForUpdateAsync(_currentVersion);

            _updateAsset = result.Asset;
            _releaseUri = result.ReleaseUri;
            AvailableVersion = result.LatestVersion.ToString(3);
            UpdateState = result.IsUpdateAvailable ? UpdateState.Available : UpdateState.UpToDate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates");
            UpdateErrorMessage = ex.Message;
            UpdateState = UpdateState.Error;
        }
    }

    [RelayCommand]
    private void ShowReleaseNotes()
    {
        if (_releaseUri is not null)
            _processHelper.Start(_releaseUri.AbsoluteUri, null, false);
    }

    [RelayCommand]
    private async Task DownloadAndInstallAsync()
    {
        if (_updateAsset is null)
            return;

        using var cts = new CancellationTokenSource();
        _downloadCts = cts;

        DownloadProgress = 0;
        DownloadProgressText = string.Empty;
        UpdateErrorMessage = string.Empty;
        UpdateState = UpdateState.Downloading;

        var progress = new Progress<UpdateDownloadProgress>(value =>
        {
            DownloadProgress = value.Percentage;
            DownloadProgressText = $"{FormatMegabytes(value.BytesReceived)} / {FormatMegabytes(value.TotalBytes)}";
        });

        try
        {
            await _updateService.DownloadUpdateAsync(_updateAsset, progress, cts.Token);
            DownloadProgress = 100;
            UpdateState = UpdateState.Available;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            UpdateState = UpdateState.Available;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download update");
            UpdateErrorMessage = ex.Message;
            UpdateState = UpdateState.Error;
        }
        finally
        {
            _downloadCts = null;
        }
    }

    [RelayCommand]
    private void CancelUpdate()
    {
        _downloadCts?.Cancel();
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        _processHelper.Start(GitHubUri, null, false);
    }

    private string FormatMegabytes(long bytes)
    {
        return $"{bytes / 1024d / 1024d:N1} MB";
    }
}

public enum AppTheme
{
    System,
    Light,
    Dark
}

public enum AnimationMode
{
    Auto,
    On,
    Off
}

public enum UpdateState
{
    NotChecked,
    Checking,
    UpToDate,
    Available,
    Downloading,
    Installing,
    Error
}
