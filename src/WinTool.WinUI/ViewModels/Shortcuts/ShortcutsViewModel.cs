using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using WinTool.WinUI.Services;

namespace WinTool.WinUI.ViewModels.Shortcuts;

public partial class ShortcutsViewModel(ShortcutsService shortcutsService, DialogService dialogService) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } = 
        [.. shortcutsService.AvailableCommands.Select(ac => new ShortcutViewModel(shortcutsService, ac.Value, dialogService))];
}
