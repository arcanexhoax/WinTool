using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private readonly SemaphoreSlim _semaphore = new(1);

        public DelegateCommand OpenWindowCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public MainViewModel(Window window)
        {
            KeyHooker hooker = new(Key.E, Key.C);
            hooker.KeyHooked += OnKeyHooked;

            OpenWindowCommand = new DelegateCommand(() => window.Show());
            CloseWindowCommand = new DelegateCommand(() => Application.Current.Shutdown());
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
