using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using WinTool.Models;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public class RunWithArgsViewModel : ObservableObject, IDialogViewModel<string, RunWithArgsOutput>
{
    private Action<Result<RunWithArgsOutput>>? _onResult;

    public string? FileName
    {
        get; set => SetProperty(ref field, value);
    }

    public string? FullFilePath
    {
        get; set => SetProperty(ref field, value);
    }

    public string? Args
    {
        get; set => SetProperty(ref field, value);
    }

    public bool RunAsAdmin
    {
        get; set => SetProperty(ref field, value);
    }

    public bool AreOptionsOpened
    {
        get; set => SetProperty(ref field, value);
    }

    public bool IsTextSelected
    {
        get; set => SetProperty(ref field, value);
    }

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
