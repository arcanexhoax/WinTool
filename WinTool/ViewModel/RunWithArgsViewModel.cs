﻿using Prism.Commands;
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

        public DelegateCommand RunCommand { get; }
        public DelegateCommand<Window> WindowLoadedCommand { get; }
        public DelegateCommand WindowClosingCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public RunWithArgsViewModel(string filePath, string lastArgs, Action<RunWithArgsResult> result)
        {
            FileName = Path.GetFileName(filePath);
            FullFilePath = filePath;

            var folders = FullFilePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            ShortedFilePath = folders.Length > 3 ? Path.Combine(folders[0], folders[1], "...", folders[^1]) : FullFilePath;

            Args = lastArgs;

            RunCommand = new DelegateCommand(() =>
            {
                result?.Invoke(new RunWithArgsResult(true, Args));
                _window?.Close();
            });
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            WindowClosingCommand = new DelegateCommand(() => result?.Invoke(new RunWithArgsResult(false, null)));
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }
    }
}
