using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WinTool.Model;
using WinTool.Properties;
using WinTool.Utils;

namespace WinTool.ViewModel;

public enum SizeUnit : long
{
    B  = 1,
    KB = 1024,
    MB = 1_048_576,
    GB = 1_073_741_824,
    TB = 1_099_511_627_776,
}

public class CreateFileViewModel : ObservableObject, IModalViewModel<string, CreateFileOutput>
{
    private Action<Result<CreateFileOutput>>? _onResult;

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
    public RelayCommand CloseWindowCommand { get; }

    public CreateFileViewModel()
    {
        SelectedSizeUnit = SizeUnit.B;
        SizeUnits = new ObservableCollection<SizeUnit>(Enum.GetValues<SizeUnit>());

        CreateCommand = new RelayCommand(CreateFile);
        CloseWindowCommand = new RelayCommand(() => _onResult?.Invoke(new Result<CreateFileOutput>(false)));
    }

    public void OnShow(string folderPath, Action<Result<CreateFileOutput>> onResult)
    {
        _onResult = onResult;

        var di = new DirectoryInfo(folderPath);
        RelativeFolderPath = di.Name.TrimEnd(Path.DirectorySeparatorChar);

        FullFolderPath = folderPath;
        IsTextSelected = true;
        AreOptionsOpened = Size > 0;
    }

    public void OnClose() => _onResult = null;

    private void CreateFile()
    {
        if (FileName is null or [] || FullFolderPath is null or [])
            return;

        long sizeBytes = Size * (long)SelectedSizeUnit;
        string filePath = Path.Combine(FullFolderPath, FileName);

        if (!CheckIfFileValid(filePath, FileName, sizeBytes, out string? errorMessage))
        {
            MessageBoxHelper.ShowError(errorMessage);
            return;
        }

        _onResult?.Invoke(new Result<CreateFileOutput>(true, new CreateFileOutput(filePath, sizeBytes)));
    }

    private bool CheckIfFileValid(string filePath, string fileName, long sizeBytes, out string? errorMessage)
    {
        if (fileName.All(c => c == '.'))
        {
            errorMessage = string.Format(Resources.FileConsistsOnlyOfDots, filePath);
            return false;
        }

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            errorMessage = string.Format(Resources.FileHasForbiddenChars, filePath);
            return false;
        }

        if (File.Exists(filePath))
        {
            errorMessage = string.Format(Resources.FileAlreadyExists, filePath);
            return false;
        }

        string? driveLetter = Path.GetPathRoot(filePath);
        if (string.IsNullOrEmpty(driveLetter))
        {
            errorMessage = string.Format(Resources.FilePathInvalid, filePath);
            return false;
        }

        var drive = DriveInfo.GetDrives().FirstOrDefault(d => string.Equals(d.Name, driveLetter, StringComparison.InvariantCultureIgnoreCase));
        if (drive is null)
        {
            errorMessage = string.Format(Resources.DriveNotFound, driveLetter);
            return false;
        }

        if (drive.AvailableFreeSpace < sizeBytes)
        {
            errorMessage = string.Format(Resources.OutOfMemory, driveLetter, drive.AvailableFreeSpace, sizeBytes);
            return false;
        }

        errorMessage = null;
        return true;
    }
}
