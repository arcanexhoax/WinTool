using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.WinUI.Models;

namespace WinTool.WinUI.ViewModels;

public partial class MainViewModel(AppState appState) : ObservableObject
{
    private readonly AppState _appState = appState;

    public void OnWindowHidden() => _appState.IsBackgroundMode = true;
}
