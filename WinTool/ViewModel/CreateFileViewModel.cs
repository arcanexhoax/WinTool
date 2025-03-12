using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WinTool.Model;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

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

    public class CreateFileViewModel : ObservableObject
    {
        private Window? _window;

        public string? FileName
        {
            get; set => SetProperty(ref field, value);
        }

        public string? FullFolderPath
        {
            get; set => SetProperty(ref field, value);
        }

        public string? RelativeFolderPath
        {
            get; set => SetProperty(ref field, value);
        }

        public uint Size
        {
            get; set => SetProperty(ref field, value);
        }

        public bool IsTextSelected
        {
            get; set => SetProperty(ref field, value);
        }

        public bool AreOptionsOpened
        {
            get; set => SetProperty(ref field, value);
        }

        public SizeUnit SelectedSizeUnit
        {
            get; set => SetProperty(ref field, value);
        }

        public ObservableCollection<SizeUnit> SizeUnits
        {
            get; set => SetProperty(ref field, value);
        }

        public RelayCommand CreateCommand { get; }
        public RelayCommand<Window> WindowLoadedCommand { get; }
        public RelayCommand WindowClosingCommand { get; }
        public RelayCommand CloseWindowCommand { get; }

        public CreateFileViewModel(string folderPath, MemoryCache memoryCache, Action<CreateFileResult> result)
        {
            SelectedSizeUnit = SizeUnit.B;
            SizeUnits = new ObservableCollection<SizeUnit>(Enum.GetValues<SizeUnit>());
            FullFolderPath = folderPath;

            var di = new DirectoryInfo(folderPath);
            RelativeFolderPath = di.Name.TrimEnd(Path.DirectorySeparatorChar);

            if (memoryCache.TryGetValue(nameof(CreateFileViewModel), out CreateFileData? createFileData))
            {
                FileName = createFileData!.FileName;
                IsTextSelected = true;

                Size = createFileData.Size;
                SelectedSizeUnit = createFileData.SizeUnit;
                AreOptionsOpened = Size > 0;
            }

            var createFileResult = new CreateFileResult(false, null);

            CreateCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(FileName))
                    return;

                long sizeBytes = Size * (long)SelectedSizeUnit;
                string filePath = Path.Combine(folderPath, FileName);

                if (!CheckIfFileValid(filePath, FileName, sizeBytes, out string? errorMessage))
                {
                    MessageBoxHelper.ShowError(errorMessage);
                    return;
                }

                memoryCache.Set(nameof(CreateFileViewModel), new CreateFileData(FileName, Size, SelectedSizeUnit));
                createFileResult = new CreateFileResult(true, filePath, sizeBytes);

                _window?.Close();
            });
            WindowLoadedCommand = new RelayCommand<Window>(w => _window = w);
            WindowClosingCommand = new RelayCommand(() => result?.Invoke(createFileResult));
            CloseWindowCommand = new RelayCommand(() => _window?.Close());
        }

        private bool CheckIfFileValid(string filePath, string fileName, long sizeBytes, out string? errorMessage)
        {
            if (fileName.All(c => c == '.'))
            {
                errorMessage = string.Format(Resource.FileConsistsOnlyOfDots, filePath);
                return false;
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorMessage = string.Format(Resource.FileHasForbiddenChars, filePath);
                return false;
            }

            if (File.Exists(filePath))
            {
                errorMessage = string.Format(Resource.FileAlreadyExists, filePath);
                return false;
            }

            string? driveLetter = Path.GetPathRoot(filePath);
            if (string.IsNullOrEmpty(driveLetter))
            {
                errorMessage = string.Format(Resource.FilePathInvalid, filePath);
                return false;
            }

            var drive = DriveInfo.GetDrives().FirstOrDefault(d => string.Equals(d.Name, driveLetter, StringComparison.InvariantCultureIgnoreCase));
            if (drive is null)
            {
                errorMessage = string.Format(Resource.DriveNotFound, driveLetter);
                return false;
            }

            if (drive.AvailableFreeSpace < sizeBytes)
            {
                errorMessage = string.Format(Resource.OutOfMemory, driveLetter, drive.AvailableFreeSpace, sizeBytes);
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
