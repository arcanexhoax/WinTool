using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using WinTool.WinUI.Models;
using WinTool.WinUI.Properties;
using WinTool.WinUI.Services;

namespace WinTool.WinUI.ViewModels.Shortcuts;

public partial class ShortcutViewModel(ShortcutsService shortcutsService, ShortcutCommand shortcutCommand, DialogService dialogService) : ObservableObject
{
    private readonly string _id = shortcutCommand.Id;
    private readonly ShortcutsService _shortcutsService = shortcutsService;
    private readonly DialogService _dialogService = dialogService;

    [ObservableProperty]
    public partial Shortcut? Shortcut { get; set; } = shortcutCommand.Shortcut;

    public string Icon { get; } = (string)Application.Current.Resources[$"Icon.{shortcutCommand.Id}"];

    public string Description { get; } = Resources.ResourceManager.GetString(shortcutCommand.Id)!;

    [RelayCommand]
    private async Task EditShortcutAsync()
    {
        var input = new EditShortcutInput(Shortcut, _id);
        var result = await _dialogService.EditShortcutAsync(input);

        if (result is not { Success: true, Data: { } newShortcut } || Shortcut == newShortcut)
            return;

        _shortcutsService.EditShortcut(_id, newShortcut);
        Shortcut = newShortcut;
    }
}
