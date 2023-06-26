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
            // use arg "-background 1" to start app in background mode
            _executionFilePath =  $"{Path.Combine(exeFolderPath!, "WinTool.exe")} -background 1";

            _keyHooker = new(Key.C, Key.E, Key.L, Key.O);
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
                case Key.E:
                    if (e.Modifier.HasFlag(KeyModifier.Ctrl))
                    {
                        string? path = await Shell.GetActiveExplorerPathAsync();

                        if (string.IsNullOrEmpty(path))
                            return;

                        if (e.Modifier.HasFlag(KeyModifier.Shift))
                        {
                            DirectoryInfo di = new(path);
                            int num = 0;
                            string? fileName = Path.GetFileNameWithoutExtension(NewFileTemplate);
                            string? extension = Path.GetExtension(NewFileTemplate);

                            try
                            {
                                var numbers = di.EnumerateFiles($"{fileName}_*{extension}").Select(f =>
                                {
                                    var match = Regex.Match(f.Name, $@"^{fileName}_(\d+){extension}$");

                                    if (match.Groups.Count != 2)
                                        return -1;

                                    if (int.TryParse(match.Groups[1].Value, out int number))
                                        return number;
                                    return -1;
                                });

                                if (numbers.Any())
                                    num = numbers.Max() + 1;

                                using (File.Create(Path.Combine(path, $"{fileName}_{num}{extension}"))) { }
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
                    break;
                case Key.C:
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
                    break;
                case Key.O:
                    if (e.Modifier.HasFlag(KeyModifier.Ctrl))
                    {
                        var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

                        if (selectedPaths.Count != 1 || Path.GetExtension(selectedPaths[0]) != ".exe")
                            return;

                        string selectedItem = selectedPaths[0];
                        RunWithArgsViewModel runWithArgsVm = new(selectedPaths[0], r =>
                        {
                            if (r.Success)
                            {
                                if (File.Exists(selectedItem))
                                    using (Process.Start(selectedItem, r.Args ?? string.Empty)) { }
                                else
                                    MessageBox.Show($"File '{selectedItem}' doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });

                        RunWithArgsWindow runWithArgsWindow = new(runWithArgsVm);
                        runWithArgsWindow.Show();
                        runWithArgsWindow.Activate();
                    }
                    break;
                case Key.L:
                    if (e.Modifier.HasFlag(KeyModifier.Ctrl) && e.Modifier.HasFlag(KeyModifier.Shift))
                    {
                        string? folderPath = await Shell.GetActiveExplorerPathAsync();

                        if (string.IsNullOrEmpty(folderPath))
                            return;

                        using (Process.Start(new ProcessStartInfo
                        {
                            WorkingDirectory = folderPath,
                            FileName = "cmd.exe",
                            UseShellExecute = false,
                        })) { }
                    }
                    break;
            }
            
            _semaphore.Release();
        }
    }
}
