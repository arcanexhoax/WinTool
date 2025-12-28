using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WinTool.Services;
using WinTool.WinUI;

namespace WinTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }
    public RelayCommand OpenWindowCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public event EventHandler? ShowWindowRequested;

    public MainViewModel(
        ShellCommandHandler commandHandler)
    {
        WindowLoadedCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => commandHandler.IsBackgroundMode = true);
        OpenWindowCommand = new RelayCommand(() =>
        {
            commandHandler.IsBackgroundMode = false;
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        });
        CloseWindowCommand = new RelayCommand(App.Current.Exit);
    }
}
