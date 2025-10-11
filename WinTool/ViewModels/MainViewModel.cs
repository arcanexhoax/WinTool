using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;
using WinTool.ViewModels.Features;
using WinTool.ViewModels.Settings;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly KeyInterceptor _keyInterceptor;
    private readonly ShellCommandHandler _commandHandler;
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
        ShellCommandHandler commandHandler,
        Shell shell,
        KeyInterceptor keyInterceptor,
        ShortcutsViewModel shortcutsViewModel,
        FeaturesViewModel featuresViewModel,
        SettingsViewModel settingsViewModel)
    {
        _commandHandler = commandHandler;
        _shell = shell;
        _keyInterceptor = keyInterceptor;

        var shortcuts = new Dictionary<Shortcut, Action>()
        {
            { new Shortcut(Key.F2, KeyModifier.Ctrl, KeyState.Down), _commandHandler.ChangeFileProperties },
            { new Shortcut(Key.C, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), _commandHandler.CopyFilePath },
            { new Shortcut(Key.X, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), _commandHandler.CopyFileName },
            { new Shortcut(Key.E, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), _commandHandler.CreateFileFast },
            { new Shortcut(Key.E, KeyModifier.Ctrl, KeyState.Down), _commandHandler.CreateFileInteractive },
            { new Shortcut(Key.L, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), _commandHandler.OpenInCmd },
            { new Shortcut(Key.O, KeyModifier.Ctrl, KeyState.Down), _commandHandler.RunWithArgs },
        };

        foreach (var (sc, handler) in shortcuts)
        {
            _keyInterceptor.RegisterShortcut(sc, () => ExecuteCommand(sc, handler));
        }

        WindowLoadedCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = true);
        OpenWindowCommand = new RelayCommand(() =>
        {
            commandHandler.IsBackgroundMode = false;
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        });
        CloseWindowCommand = new RelayCommand(() =>
        {
            _keyInterceptor?.Dispose();
            Application.Current.Shutdown();
        });

        ShortcutsViewModel = shortcutsViewModel;
        FeaturesViewModel = featuresViewModel;
        SettingsViewModel = settingsViewModel;
    }

    private bool ExecuteCommand(Shortcut shortcut, Action command)
    {
        Debug.WriteLine($"Executing {shortcut}");
        var shellActive = _shell.IsActive;

        // use a new thread because it is unable to get shell windows from MTA thread
        Thread t = new(() =>
        {
            try
            {
                command();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to execute command {shortcut}: {ex.Message}");
                // TODO fix: message box is minimized
                MessageBoxHelper.ShowError(string.Format(Resources.CommandExecutionError, shortcut, ex.Message));
            }
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        return shellActive;
    }
}
