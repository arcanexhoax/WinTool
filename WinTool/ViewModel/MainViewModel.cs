using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Services;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private const string RegKeyName = "WinTool";

        private readonly KeyInterceptor _keyHooker;
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly SettingsManager _settingsManager;
        private readonly Settings _settings;
        private readonly Shell _shell;
        private readonly Dictionary<Shortcut, Func<Task>> _shortcuts;
        private readonly string _executionFilePath;

        public bool LaunchOnWindowsStartup
        {
            get;
            set
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
                    MessageBoxHelper.ShowError(string.Format(Resource.SetWindowsStartupError, ex.Message));
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
            get;
            set
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

        public MainViewModel(CommandHandler commandHandler, SettingsManager settingsManager, Shell shell, LanguagePopupHandler languagePopupHandler)
        {
            _shortcuts = new()
            {
                { new Shortcut(Key.F2, KeyModifier.Ctrl, KeyState.Down),                    commandHandler.ChangeFileProperties },
                { new Shortcut(Key.C, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), commandHandler.CopyFilePath },
                { new Shortcut(Key.E, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), () => commandHandler.CreateFileFast(NewFileTemplate!) },
                { new Shortcut(Key.E, KeyModifier.Ctrl, KeyState.Down),                     commandHandler.CreateFileInteractive },
                { new Shortcut(Key.L, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), commandHandler.OpenInCmd },
                { new Shortcut(Key.O, KeyModifier.Ctrl, KeyState.Down),                     commandHandler.RunWithArgs },
                { new Shortcut(Key.X, KeyModifier.Ctrl | KeyModifier.Shift, KeyState.Down), commandHandler.CopyFileName },
            };

            // use arg "/background" to start app in background mode
            _executionFilePath =  $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";
            _settingsManager = settingsManager;
            _shell = shell;

            _keyHooker = new KeyInterceptor(_shortcuts.Keys);
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

            _settings = _settingsManager.GetSettings() ?? new Settings();
            LaunchOnWindowsStartup = _settings.WindowsStartupEnabled;
            NewFileTemplate = _settings.NewFileTemplate;
        }

        private async void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
        {
            await _semaphore.WaitAsync();

            Debug.WriteLine($"{e.Shortcut} // {e.Shortcut.State}");

            if (_shortcuts.TryGetValue(e.Shortcut, out Func<Task>? operation) && operation is not null)
            {
                try
                {
                    if (_shell.IsActive)
                        e.IsHandled = true;

                    await operation();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    MessageBoxHelper.ShowError(ex.Message);
                }
            }

            _semaphore.Release();
        }
    }
}
