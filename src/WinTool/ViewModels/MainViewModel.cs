using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTool.Models;

namespace WinTool.ViewModels;

public class MainViewModel : ObservableObject
{
    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }

    public MainViewModel(AppState appState)
    {
        WindowLoadedCommand = new RelayCommand(() => appState.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => appState.IsBackgroundMode = true);
    }
}
