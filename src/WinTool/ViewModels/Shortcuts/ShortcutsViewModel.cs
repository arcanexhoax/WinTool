using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
using System.Collections.ObjectModel;
using System.Linq;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;
using WinTool.UI;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutsViewModel(
    ShortcutsService shortcutsService,
    ViewFactory viewFactory) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } = 
        [.. shortcutsService.Shortcuts.Select(sc => new ShortcutViewModel(shortcutsService, sc.Value, viewFactory))];
}
