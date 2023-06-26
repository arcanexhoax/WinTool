using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public class RunWithArgsViewModel : BindableBase
    {
        private const string TitleTemplate = "Run '{0}'";
        private const string DescriptionTemplate = "Run '{0}' with args:";

        private Window? _window;
        private string? _fileName;
        private string? _filePath;
        private string? _args;

        public string? FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string? FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
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

        public RunWithArgsViewModel(string filePath, Action<RunWithArgsResult> result)
        {
            FilePath = string.Format(DescriptionTemplate, Path.GetFileName(filePath));
            FileName = string.Format(TitleTemplate, filePath);

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
