using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using WinTool.Models;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public class RunWithArgsViewModel : ObservableObject, IDialogViewModel<string, string?>
{
    private Action<Result<string?>>? _onResult;

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

    public bool IsTextSelected
    {
        get; set => SetProperty(ref field, value);
    }

    public RelayCommand RunCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public RunWithArgsViewModel()
    {
        RunCommand = new RelayCommand(() => _onResult?.Invoke(new Result<string?>(true, Args)));
        CloseWindowCommand = new RelayCommand(() => _onResult?.Invoke(new Result<string?>(false)));
    }

    public void OnShow(string filePath, Action<Result<string?>> onResult)
    {
        _onResult = onResult;

        FileName = Path.GetFileName(filePath);
        FullFilePath = filePath;
        IsTextSelected = true;
    }

    public void OnClose() => _onResult = null;
}
