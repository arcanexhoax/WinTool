using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using WinTool.Models;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public partial class RunWithArgsViewModel : ObservableObject, IDialogViewModel<string, RunWithArgsOutput>
{
    private Action<Result<RunWithArgsOutput>>? _onResult;

    [ObservableProperty]
    public partial string? FileName { get; set; }

    [ObservableProperty]
    public partial string? FullFilePath { get; set; }

    [ObservableProperty]
    public partial string? Args { get; set; }

    [ObservableProperty]
    public partial bool RunAsAdmin { get; set; }

    [ObservableProperty]
    public partial bool AreOptionsOpened { get; set; }

    [ObservableProperty]
    public partial bool IsTextSelected { get; set; }

    public RelayCommand RunCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public RunWithArgsViewModel()
    {
        RunCommand = new RelayCommand(() => _onResult?.Invoke(new Result<RunWithArgsOutput>(true, new RunWithArgsOutput(Args, RunAsAdmin))));
        CloseWindowCommand = new RelayCommand(() => _onResult?.Invoke(new Result<RunWithArgsOutput>(false)));
    }

    public void OnShow(string filePath, Action<Result<RunWithArgsOutput>> onResult)
    {
        _onResult = onResult;

        FileName = Path.GetFileName(filePath);
        FullFilePath = filePath;
        IsTextSelected = true;
        AreOptionsOpened = RunAsAdmin;
    }

    public void OnClose() => _onResult = null;
}
