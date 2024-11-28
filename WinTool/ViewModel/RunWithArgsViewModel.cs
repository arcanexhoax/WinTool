using Microsoft.Extensions.Caching.Memory;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows;
using WinTool.Model;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class RunWithArgsViewModel : BindableBase
    {
        private Window? _window;

        public string? FileName
        {
            get; set => SetProperty(ref field, value);
        }

        public string? FullFilePath
        {
            get; set => SetProperty(ref field, value);
        }

        public string? ShortedFilePath
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Args
        {
            get; set => SetProperty(ref field, value);
        }

        public bool IsTextSelected
        {
            get; set => SetProperty(ref field, value);
        }

        public DelegateCommand RunCommand { get; }
        public DelegateCommand<Window> WindowLoadedCommand { get; }
        public DelegateCommand WindowClosingCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public RunWithArgsViewModel(string filePath, MemoryCache memoryCache, Action<RunWithArgsResult> result)
        {
            FileName = Path.GetFileName(filePath);
            FullFilePath = filePath;

            var folders = FullFilePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            ShortedFilePath = folders.Length > 3 ? Path.Combine(folders[0], folders[1], "...", folders[^1]) : FullFilePath;

            if (memoryCache.TryGetValue(nameof(RunWithArgsViewModel), out string? lastArgs))
            {
                Args = lastArgs;
                IsTextSelected = true;
            }

            var runWithArgsResult = new RunWithArgsResult(false, null);

            RunCommand = new DelegateCommand(() =>
            {
                if (!File.Exists(FullFilePath))
                    MessageBoxHelper.ShowError(string.Format(Resource.FileNotFound, FullFilePath));
                else
                    runWithArgsResult = new RunWithArgsResult(true, Args);

                memoryCache.Set(nameof(RunWithArgsViewModel), Args);
                _window?.Close();
            });
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            WindowClosingCommand = new DelegateCommand(() => result?.Invoke(runWithArgsResult));
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }
    }
}
