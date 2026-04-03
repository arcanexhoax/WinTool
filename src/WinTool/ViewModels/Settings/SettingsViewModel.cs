using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Options;
using WinTool.Properties;

namespace WinTool.ViewModels.Settings;

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

public class SettingsViewModel : ObservableObject
{
    private const string RegKeyName = "WinTool";

    private readonly string _executionFilePath;
    private readonly ILogger _logger;
    private readonly WritableOptions<SettingsOptions> _settingsOptions;

    private bool _isInitializing;

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

    public SettingsViewModel(ILogger<SettingsViewModel> logger, WritableOptions<SettingsOptions> settingsOptions)
    {
        // use arg "/background" to start app in background mode
        _executionFilePath = $"{Environment.ProcessPath!} {BackgroundParameter.ParameterName}";
        _logger = logger;
        _settingsOptions = settingsOptions;
        _isInitializing = true;

        LaunchOnWindowsStartup = _settingsOptions.CurrentValue.WindowsStartupEnabled;
        AlwaysRunAsAdmin = _settingsOptions.CurrentValue.AlwaysRunAsAdmin;
        SelectedAppTheme = _settingsOptions.CurrentValue.AppTheme;
        SelectedAnimationMode = _settingsOptions.CurrentValue.AnimationMode;
        SelectedLanguage = _settingsOptions.CurrentValue.Language;

        _isInitializing = false;
    }
}
