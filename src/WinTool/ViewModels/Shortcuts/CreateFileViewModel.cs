using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Properties;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public partial class CreateFileViewModel : ObservableObject, IDialogViewModel<string, CreateFileOutput>
{
    private readonly ILogger _logger;
    private readonly CreateFileDialogState _state;

    private Action<Result<CreateFileOutput>>? _onResult;

    [ObservableProperty]
    public partial string? FileName { get; set; }

    [ObservableProperty]
    public partial string? FullFolderPath { get; set; }

    [ObservableProperty]
    public partial string? RelativeFolderPath { get; set; }

    [ObservableProperty]
    public partial uint Size { get; set; }

    [ObservableProperty]
    public partial bool IsTextSelected { get; set; }

    [ObservableProperty]
    public partial bool AreOptionsOpened { get; set; }

    [ObservableProperty]
    public partial SizeUnit SelectedSizeUnit { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SizeUnit> SizeUnits { get; set; }

    public RelayCommand CreateCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public CreateFileViewModel(ILogger<CreateFileViewModel> logger, CreateFileDialogState state)
    {
        _logger = logger;
        _state = state;

        SizeUnits = new ObservableCollection<SizeUnit>(Enum.GetValues<SizeUnit>());
        FileName = _state.FileName;
        Size = _state.Size;
        SelectedSizeUnit = _state.SelectedSizeUnit;

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

    public void OnClose()
    {
        _state.FileName = FileName;
        _state.Size = Size;
        _state.SelectedSizeUnit = SelectedSizeUnit;
        _onResult = null;
    }

    private void CreateFile()
    {
        if (FileName is null or [] || FullFolderPath is null or [])
            return;

        long sizeBytes = Size * (long)SelectedSizeUnit;
        var result = ValidateCreateFileOutput(FullFolderPath, FileName, sizeBytes);

        if (!result.Success)
        {
            _logger.LogInformation("File is not valid: {ErrorMessage}", result.Message);
            MessageBox.ShowError(result.Message);
            return;
        }

        _onResult?.Invoke(result);
    }

    internal static Result<CreateFileOutput> ValidateCreateFileOutput(string folderPath, string fileName, long sizeBytes)
    {
        string filePath = Path.Combine(folderPath, fileName);

        if (fileName.All(c => c == '.'))
        {
            return new(false, Message: string.Format(Resources.FileConsistsOnlyOfDots, filePath));
        }

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return new(false, Message: string.Format(Resources.FileHasForbiddenChars, filePath));
        }

        if (File.Exists(filePath))
        {
            return new(false, Message: string.Format(Resources.FileAlreadyExists, filePath));
        }

        string? driveLetter = Path.GetPathRoot(filePath);
        if (string.IsNullOrEmpty(driveLetter))
        {
            return new(false, Message: string.Format(Resources.FilePathInvalid, filePath));
        }

        var drive = DriveInfo.GetDrives().FirstOrDefault(d => string.Equals(d.Name, driveLetter, StringComparison.InvariantCultureIgnoreCase));
        if (drive is null)
        {
            return new(false, Message: string.Format(Resources.DriveNotFound, driveLetter));
        }

        if (drive.AvailableFreeSpace < sizeBytes)
        {
            return new(false, Message: string.Format(Resources.OutOfMemory, driveLetter, drive.AvailableFreeSpace, sizeBytes));
        }

        return new(true, new CreateFileOutput(filePath, sizeBytes));
    }
}
