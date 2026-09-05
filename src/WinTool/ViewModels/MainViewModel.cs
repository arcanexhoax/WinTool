using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using WinTool.Models;

namespace WinTool.ViewModels;

public class MainViewModel : ObservableObject
{
    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }
    public RelayCommand OpenWindowCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public event EventHandler? ShowWindowRequested;

    public MainViewModel(AppState appState)
    {
        WindowLoadedCommand = new RelayCommand(() => appState.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => appState.IsBackgroundMode = true);
        OpenWindowCommand = new RelayCommand(() =>
        {
            appState.IsBackgroundMode = false;
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        });
        CloseWindowCommand = new RelayCommand(Application.Current.Shutdown);
    }
}
