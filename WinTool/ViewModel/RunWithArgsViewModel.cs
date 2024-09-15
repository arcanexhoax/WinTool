using Microsoft.Extensions.Caching.Memory;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows;
using WinTool.Model;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class RunWithArgsViewModel : BindableBase
    {
        private Window? _window;
        private string? _fileName;
        private string? _fullFilePath;
        private string? _shortedFilePath;
        private string? _args;
        private bool _isTextSelected;

        public string? FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string? FullFilePath
        {
            get => _fullFilePath;
            set => SetProperty(ref _fullFilePath, value);
        }

        public string? ShortedFilePath
        {
            get => _shortedFilePath;
            set => SetProperty(ref _shortedFilePath, value);
        }

        public string? Args
        {
            get => _args;
            set => SetProperty(ref _args, value);
        }

        public bool IsTextSelected
        {
            get => _isTextSelected;
            set => SetProperty(ref _isTextSelected, value);
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
                    MessageBox.Show(string.Format(Resource.FileNotFound, FullFilePath), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
