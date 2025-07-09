using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;

namespace WinTool.ViewModel;

public class SettingsViewModel : ObservableObject
{
    private const string RegKeyName = "WinTool";

    private readonly string _executionFilePath;
    private readonly SettingsManager _settingsManager;
    private readonly Settings _settings;

    public bool LaunchOnWindowsStartup
    {
        get; set
        {
            try
            {
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

                _settings.WindowsStartupEnabled = value;
                _settingsManager.UpdateSettings(_settings);

                SetProperty(ref field, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting Windows startup: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Resources.SetWindowsStartupError, ex.Message));
            }
        }
    }

    public SettingsViewModel(SettingsManager settingsManager)
    {
        // use arg "/background" to start app in background mode
        _executionFilePath = $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";
        _settingsManager = settingsManager;
        _settings = _settingsManager.GetSettings() ?? new Settings();

        LaunchOnWindowsStartup = _settings.WindowsStartupEnabled;
    }
}
