using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public class CreateFileViewModel : BindableBase
    {
        private string? _fileName;
        private string? _relativeFolderPath;
        private Window? _window;

        public string? FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string? RelativeFolderPath
        {
            get => _relativeFolderPath;
            set => SetProperty(ref _relativeFolderPath, value);
        }

        public DelegateCommand CreateCommand { get; }
        public DelegateCommand<Window> WindowLoadedCommand { get; }
        public DelegateCommand WindowClosingCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public CreateFileViewModel(string folderPath, Action<CreateFileResult> result)
        {
            DirectoryInfo di = new(folderPath);
            string folderName = di.Name.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (folderName.Length > 17)
            {
                int start = folderName.Length - 17;
                RelativeFolderPath = string.Concat("...", folderName.AsSpan(start));
            }
            else
            {
                RelativeFolderPath = folderName;
            }

            CreateCommand = new DelegateCommand(() =>
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    result?.Invoke(new CreateFileResult(true, Path.Combine(folderPath, FileName)));
                    _window?.Close();
                }
            });
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            WindowClosingCommand = new DelegateCommand(() => result?.Invoke(new CreateFileResult(false, null)));
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }
    }
}
