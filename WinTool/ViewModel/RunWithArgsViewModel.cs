using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Windows;
using WinTool.Model;
using WinTool.Properties;
using WinTool.Utils;

namespace WinTool.ViewModel;

public class RunWithArgsViewModel : ObservableObject
{
    private Window? _window;

    public string? FileName
    {
        get; set => SetProperty(ref field, value);
    }

    public string? FullFilePath
    {
        get; set => SetProperty(ref field, value);
    }

    public string? ShortedFilePath
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
    public RelayCommand<Window> WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public RunWithArgsViewModel(string filePath, MemoryCache memoryCache, Action<RunWithArgsResult> result)
    {
        FileName = Path.GetFileName(filePath);
        FullFilePath = filePath;

        var folders = FullFilePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        ShortedFilePath = folders.Length > 3 ? Path.Combine(folders[0], folders[1], "...", folders[^1]) : FullFilePath;

        if (memoryCache.TryGetValue(nameof(RunWithArgsViewModel), out string? lastArgs))
        {
            Args = lastArgs;
            IsTextSelected = true;
        }

        var runWithArgsResult = new RunWithArgsResult(false, null);

        RunCommand = new RelayCommand(() =>
        {
            if (!File.Exists(FullFilePath))
                MessageBoxHelper.ShowError(string.Format(Resources.FileNotFound, FullFilePath));
            else
                runWithArgsResult = new RunWithArgsResult(true, Args);

            memoryCache.Set(nameof(RunWithArgsViewModel), Args);
            _window?.Close();
        });
        WindowLoadedCommand = new RelayCommand<Window>(w => _window = w);
        WindowClosingCommand = new RelayCommand(() => result?.Invoke(runWithArgsResult));
        CloseWindowCommand = new RelayCommand(() => _window?.Close());
    }
}
