using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WinTool.Enum;
using WinTool.Model;
using WinTool.Modules;
using WinTool.View;

namespace WinTool.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private const string RegKeyName = "WinTool";

        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly SettingsManager _settingsManager = new();
        private readonly Settings _settings;
        private readonly string _executionFilePath;

        private bool _launchOnWindowsStartup;
        private bool _areUiElementsEnabled;

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

        public DelegateCommand OpenWindowCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public MainViewModel(Window window)
        {
            string? exeFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // use arg "-background 1" to start app in background mode
            _executionFilePath =  $"{Path.Combine(exeFolderPath!, "WinTool.exe")} -background 1";

            KeyHooker hooker = new(Key.E, Key.C);
            hooker.KeyHooked += OnKeyHooked;

            OpenWindowCommand = new DelegateCommand(() => window.Show());
            CloseWindowCommand = new DelegateCommand(() => Application.Current.Shutdown());

            _settings = _settingsManager.GetSettings() ?? new Settings();
            LaunchOnWindowsStartup = _settings.WindowsStartupEnabled;
        }

        private async void OnKeyHooked(object? sender, KeyHookedEventArgs e)
        {
            await _semaphore.WaitAsync();

            if (e.Key == Key.E)
            {
                if (e.Modifier.HasFlag(KeyModifier.Ctrl))
                {
                    string? path = await Shell.GetActiveExplorerPathAsync();

                    if (string.IsNullOrEmpty(path))
                        return;

                    if (e.Modifier.HasFlag(KeyModifier.Shift))
                    {
                        DirectoryInfo di = new(path);
                        int num = 0;

                        try
                        {
                            var numbers = di.EnumerateFiles("NewFile_*.txt").Select(f => int.Parse(Regex.Match(f.Name, @"\d+").Value));

                            if (numbers.Any())
                                num = numbers.Max() + 1;

                            using (File.Create(Path.Combine(path, $"NewFile_{num}.txt"))) { }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        CreateFileViewModel createFileVm = new(path, r => 
                        {
                            if (r.Success && !string.IsNullOrEmpty(r.FilePath))
                            {
                                if (!File.Exists(r.FilePath))
                                    using (File.Create(r.FilePath)) { }
                                else
                                    MessageBox.Show($"File '{r.FilePath}' already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });

                        CreateFileView createFileView = new(createFileVm);
                        createFileView.Show();
                        createFileView.Activate();
                    }
                }
            }
            else if (e.Key == Key.C)
            {
                if (e.Modifier.HasFlag(KeyModifier.Ctrl) && e.Modifier.HasFlag(KeyModifier.Shift))
                {
                    var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

                    // if there are no selections - copy folder path, if one item selected - copy item's path, else - not copying
                    if (selectedPaths.Count == 0)
                    {
                        string? folderPath = await Shell.GetActiveExplorerPathAsync();
                        Clipboard.SetText(folderPath);
                    }
                    else if (selectedPaths.Count == 1)
                    {
                        Clipboard.SetText(selectedPaths[0]);
                    }
                }
            }

            _semaphore.Release();
        }
    }
}
