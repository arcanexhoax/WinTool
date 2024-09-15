using GlobalKeyInterceptor;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Modules;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private const string RegKeyName = "WinTool";

        private readonly KeyInterceptor _keyHooker;
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly SettingsManager _settingsManager;
        private readonly Settings _settings;
        private readonly Dictionary<Shortcut, Func<Task>> _shortcuts;
        private readonly string _executionFilePath;

        private bool _launchOnWindowsStartup;
        private bool _areUiElementsEnabled;
        private string? _newFileTemplate;

        public bool LaunchOnWindowsStartup
        {
            get => _launchOnWindowsStartup;
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

                    SetProperty(ref _launchOnWindowsStartup, value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resource.SetWindowsStartupError, ex.Message), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    AreUiElementsEnabled = true;
                }
            }
        }

        public bool AreUiElementsEnabled
        {
            get => _areUiElementsEnabled;
            set => SetProperty(ref _areUiElementsEnabled, value);
        }

        public string? NewFileTemplate
        {
            get => _newFileTemplate;
            set
            {
                if (SetProperty(ref _newFileTemplate, value))
                {
                    _settings.NewFileTemplate = value;
                    _settingsManager.UpdateSettings(_settings);
                }
            }
        }

        public DelegateCommand WindowLoadedCommand { get; }
        public DelegateCommand WindowClosingCommand { get; }
        public DelegateCommand OpenWindowCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public event EventHandler? ShowWindowRequested;

        public MainViewModel(CommandHandler commandHandler, SettingsManager settingsManager)
        {
            _shortcuts = new()
            {
                { new Shortcut(Key.C, KeyModifier.Ctrl | KeyModifier.Shift), () => commandHandler.CopyFilePath() },
                { new Shortcut(Key.E, KeyModifier.Ctrl | KeyModifier.Shift), () => commandHandler.CreateFileFast(NewFileTemplate!) },
                { new Shortcut(Key.E, KeyModifier.Ctrl),                     () => commandHandler.CreateFileInteractive() },
                { new Shortcut(Key.L, KeyModifier.Ctrl | KeyModifier.Shift), () => commandHandler.OpenInCmd() },
                { new Shortcut(Key.O, KeyModifier.Ctrl),                     () => commandHandler.RunWithArgs() },
                { new Shortcut(Key.X, KeyModifier.Ctrl | KeyModifier.Shift), () => commandHandler.CopyFileName() },
            };

            // use arg "/background" to start app in background mode
            _executionFilePath =  $"{ProcessHelper.ProcessPath} {BackgroundParameter.ParameterName}";
            _settingsManager = settingsManager;

            _keyHooker = new KeyInterceptor(_shortcuts.Keys);
            _keyHooker.ShortcutPressed += OnShortcutPressed;

            WindowLoadedCommand = new DelegateCommand(() => commandHandler.IsBackgroundMode = false);
            WindowClosingCommand = new DelegateCommand(() => commandHandler.IsBackgroundMode = true);
            OpenWindowCommand = new DelegateCommand(() =>
            {
                commandHandler.IsBackgroundMode = false;
                ShowWindowRequested?.Invoke(this, EventArgs.Empty);
            });
            CloseWindowCommand = new DelegateCommand(() =>
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
                    await operation();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            _semaphore.Release();
        }
    }
}
