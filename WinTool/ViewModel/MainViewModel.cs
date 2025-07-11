using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;

namespace WinTool.ViewModel;

public class MainViewModel : ObservableObject
{
    private readonly KeyInterceptor _keyHooker;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly CommandHandler _commandHandler;
    private readonly Shell _shell;

    public ShortcutsViewModel ShortcutsViewModel { get; }
    public FeaturesViewModel FeaturesViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }
    public RelayCommand OpenWindowCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public event EventHandler? ShowWindowRequested;

    public MainViewModel(
        CommandHandler commandHandler,
        Shell shell,
        KeyInterceptor keyHooker,
        ShortcutsViewModel shortcutsViewModel,
        FeaturesViewModel featuresViewModel,
        SettingsViewModel settingsViewModel)
    {
        _commandHandler = commandHandler;
        _shell = shell;
        _keyHooker = keyHooker;
        _keyHooker.ShortcutPressed += OnShortcutPressed;

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

        ShortcutsViewModel = shortcutsViewModel;
        FeaturesViewModel = featuresViewModel;
        SettingsViewModel = settingsViewModel;
    }

    private async void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        await _semaphore.WaitAsync();

        Func<Task>? command = e.Shortcut switch
        {
            { Key: Key.F2, Modifier: KeyModifier.Ctrl, State: KeyState.Down } => _commandHandler.ChangeFileProperties,
            { Key: Key.C, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.CopyFilePath,
            { Key: Key.X, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.CopyFileName,
            { Key: Key.E, Modifier: KeyModifier.Ctrl | KeyModifier.Shift, State: KeyState.Down } => _commandHandler.CreateFileFast,
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
