using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using WinTool.CommandLine;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Utils;

namespace WinTool.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private const string RegKeyName = "WinTool";

    private readonly string _executionFilePath;
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
                Debug.WriteLine($"Error setting Windows startup: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Resources.SetWindowsStartupError, ex.Message));
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

    public ElementTheme SelectedAppTheme
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _settingsOptions.Update(o => o.AppTheme = value);
            }
        }
    }

    public SettingsViewModel(WritableOptions<SettingsOptions> settingsOptions)
    {
        // use arg "/background" to start app in background mode
        _executionFilePath = $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";
        _settingsOptions = settingsOptions;
        _isInitializing = true;

        LaunchOnWindowsStartup = _settingsOptions.CurrentValue.WindowsStartupEnabled;
        AlwaysRunAsAdmin = _settingsOptions.CurrentValue.AlwaysRunAsAdmin;
        SelectedAppTheme = _settingsOptions.CurrentValue.AppTheme;

        _isInitializing = false;
    }
}
