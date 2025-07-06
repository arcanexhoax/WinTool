using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;

namespace WinTool.ViewModel;

public class MainViewModel : ObservableObject
{
    private const string RegKeyName = "WinTool";

    private readonly KeyInterceptor _keyHooker;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly CommandHandler _commandHandler;
    private readonly SettingsManager _settingsManager;
    private readonly Settings _settings;
    private readonly Shell _shell;
    private readonly SwitchLanguageViewModel _switchLanguageVm;
    private readonly string _executionFilePath;

    public bool EnableSwitchLanguagePopup
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _settings.EnableSwitchLanguagePopup = value;
                _settingsManager.UpdateSettings(_settings);

                if (value)
                    _switchLanguageVm.Start();
                else
                    _switchLanguageVm.Stop();
            }
        }
    }

    public bool LaunchOnWindowsStartup
    {
        get; set
        {
            AreUiElementsEnabled = false;

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
            finally
            {
                AreUiElementsEnabled = true;
            }
        }
    }

    public bool AreUiElementsEnabled
    {
        get; set => SetProperty(ref field, value);
    }

    public string? NewFileTemplate
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _settings.NewFileTemplate = value;
                _settingsManager.UpdateSettings(_settings);
            }
        }
    }

    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }
    public RelayCommand OpenWindowCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public event EventHandler? ShowWindowRequested;

    public MainViewModel(CommandHandler commandHandler, SettingsManager settingsManager, Shell shell, SwitchLanguageViewModel switchLanguageVm, KeyInterceptor keyHooker)
    {
        _commandHandler = commandHandler;
        _settingsManager = settingsManager;
        _shell = shell;
        _switchLanguageVm = switchLanguageVm;
        _keyHooker = keyHooker;
        _keyHooker.ShortcutPressed += OnShortcutPressed;
        // use arg "/background" to start app in background mode
        _executionFilePath = $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";

        WindowLoadedCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = true);
        OpenWindowCommand = new RelayCommand(() =>
        {
            commandHandler.IsBackgroundMode = false;
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        });
        CloseWindowCommand = new RelayCommand(() =>
        {
            _keyHooker?.Dispose();
            Application.Current.Shutdown();
        });

        _settings = _settingsManager.GetSettings() ?? new Settings();
        EnableSwitchLanguagePopup = _settings.EnableSwitchLanguagePopup;
        LaunchOnWindowsStartup = _settings.WindowsStartupEnabled;
        NewFileTemplate = _settings.NewFileTemplate;
    }

    private async void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        await _semaphore.WaitAsync();

        Func<Task>? command = e.Shortcut switch
        {
            { Key: Key.F2, Modifier: KeyModifier.Ctrl, State: KeyState.Down } => _commandHandler.ChangeFileProperties,
            { Key: Key.C, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.CopyFilePath,
            { Key: Key.X, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.CopyFileName,
            { Key: Key.E, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => () => _commandHandler.CreateFileFast(NewFileTemplate!),
            { Key: Key.E, Modifier: KeyModifier.Ctrl, State: KeyState.Down } => _commandHandler.CreateFileInteractive,
            { Key: Key.L, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.OpenInCmd,
            { Key: Key.O, Modifier: KeyModifier.Ctrl, State: KeyState.Down } => _commandHandler.RunWithArgs,
            _ => null
        };

        if (command is null)
        {
            _semaphore.Release();
            return;
        }

        Debug.WriteLine($"{e.Shortcut} // {e.Shortcut.State}");

        try
        {
            if (_shell.IsActive)
                e.IsHandled = true;

            await command();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to execute command {e.Shortcut}: {ex.Message}");
            MessageBoxHelper.ShowError(string.Format(Resources.CommandExecutionError, e.Shortcut, ex.Message));
        }

        _semaphore.Release();
    }
}
