using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using WinTool.CommandLine;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Utils;

namespace WinTool.ViewModel;

public class SettingsViewModel : ObservableObject
{
    private const string RegKeyName = "WinTool";

    private readonly string _executionFilePath;
    private readonly WritableOptions<SettingsOptions> _settingsOptions;

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

                if (SetProperty(ref field, value))
                {
                    _settingsOptions.Update(() => _settingsOptions.CurrentValue.WindowsStartupEnabled = value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting Windows startup: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Resources.SetWindowsStartupError, ex.Message));
            }
        }
    }

    public SettingsViewModel(WritableOptions<SettingsOptions> settingsOptions)
    {
        // use arg "/background" to start app in background mode
        _executionFilePath = $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";
        _settingsOptions = settingsOptions;

        LaunchOnWindowsStartup = _settingsOptions.CurrentValue.WindowsStartupEnabled;
    }
}
