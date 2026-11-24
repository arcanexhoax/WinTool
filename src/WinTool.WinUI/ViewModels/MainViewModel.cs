using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTool.WinUI.Models;

namespace WinTool.WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public RelayCommand WindowLoadedCommand { get; }
    public RelayCommand WindowClosingCommand { get; }

    public MainViewModel(AppState appState)
    {
        WindowLoadedCommand = new RelayCommand(() => appState.IsBackgroundMode = false);
        WindowClosingCommand = new RelayCommand(() => appState.IsBackgroundMode = true);
    }
}
