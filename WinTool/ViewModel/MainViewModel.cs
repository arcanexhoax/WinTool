using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Enum;
using GlobalKeyInterceptor.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using WinTool.Model;
using WinTool.Modules;

namespace WinTool.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private const string RegKeyName = "WinTool";

        private readonly KeyHooker _keyHooker;
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly SettingsManager _settingsManager = new();
        private readonly Settings _settings;
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
                    MessageBox.Show($"Unable to set windows startup. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public DelegateCommand OpenWindowCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public MainViewModel(Window window)
        {
            string? exeFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // use arg "/background" to start app in background mode
            _executionFilePath =  $"{Path.Combine(exeFolderPath!, "WinTool.exe")} {CommandLineParameters.BackgroundParameter}";

            _keyHooker = new KeyHooker(ConsoleKey.C, ConsoleKey.E, ConsoleKey.L, ConsoleKey.O, ConsoleKey.X);
            _keyHooker.KeyHooked += OnKeyHooked;

            OpenWindowCommand = new DelegateCommand(() => window.Show());
            CloseWindowCommand = new DelegateCommand(() =>
            {
                _keyHooker?.Dispose();
                Application.Current.Shutdown();
            });

            _settings = _settingsManager.GetSettings() ?? new Settings();
            LaunchOnWindowsStartup = _settings.WindowsStartupEnabled;
            NewFileTemplate = _settings.NewFileTemplate;
        }

        private async void OnKeyHooked(object? sender, KeyHookedEventArgs e)
        {
            await _semaphore.WaitAsync();

            switch (e.Key)
            {
                case ConsoleKey.E when e.Modifier.HasFlag(KeyModifier.Ctrl):
                    if (e.Modifier.HasFlag(KeyModifier.Shift))
                        await CommandHandler.FastCreateFile(NewFileTemplate!);
                    else
                        await CommandHandler.CreateFile();
                    break;
                case ConsoleKey.C when e.Modifier.HasFlag(KeyModifier.Ctrl) && e.Modifier.HasFlag(KeyModifier.Shift):
                    await CommandHandler.CopyFilePath();
                    break;
                case ConsoleKey.O when e.Modifier.HasFlag(KeyModifier.Ctrl):
                    await CommandHandler.RunWithArgs();
                    break;
                case ConsoleKey.L when e.Modifier.HasFlag(KeyModifier.Ctrl) && e.Modifier.HasFlag(KeyModifier.Shift):
                    await CommandHandler.OpenInCmd();
                    break;
                case ConsoleKey.X when e.Modifier.HasFlag(KeyModifier.Ctrl) && e.Modifier.HasFlag(KeyModifier.Shift):
                    await CommandHandler.CopyFileName();
                    break;
            }
            
            _semaphore.Release();
        }
    }
}
