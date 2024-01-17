using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public enum SizeUnit : long
    {
        B  = 1,
        KB = 1024,
        MB = 1_048_576,
        GB = 1_073_741_824,
        TB = 1_099_511_627_776,
    }

    public class CreateFileViewModel : BindableBase
    {
        private string? _fileName;
        private string? _relativeFolderPath;
        private uint _size = 0;
        private SizeUnit _selectedSizeUnit = SizeUnit.B;
        private ObservableCollection<SizeUnit> _sizeUnits = new(Enum.GetValues<SizeUnit>());
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

        public uint Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public SizeUnit SelectedSizeUnit
        {
            get => _selectedSizeUnit;
            set => SetProperty(ref _selectedSizeUnit, value);
        }

        public ObservableCollection<SizeUnit> SizeUnits
        {
            get => _sizeUnits;
            set => SetProperty(ref _sizeUnits, value);
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
                    long sizeBytes = Size * (long)SelectedSizeUnit;
                    result?.Invoke(new CreateFileResult(true, Path.Combine(folderPath, FileName), sizeBytes));
                    _window?.Close();
                }
            });
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            WindowClosingCommand = new DelegateCommand(() => result?.Invoke(new CreateFileResult(false, null)));
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }
    }
}
