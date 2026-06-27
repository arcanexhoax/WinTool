using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System.Windows;
using WinTool.Models;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

public partial class ShortcutViewModel(ShortcutsService shortcutsService, ShortcutCommand shortcutCommand, ViewFactory viewFactory) : ObservableObject
{
    private readonly string _id = shortcutCommand.Id;
    private readonly ShortcutsService _shortcutsService = shortcutsService;
    private readonly ViewFactory _viewFactory = viewFactory;

    [ObservableProperty]
    public partial Shortcut? Shortcut { get; set; } = shortcutCommand.Shortcut;

    public string Icon { get; } = (string)Application.Current.FindResource($"Icon.{shortcutCommand.Id}");

    public string Description { get; } = Resources.ResourceManager.GetString(shortcutCommand.Id)!;

    [RelayCommand]
    private void EditShortcut()
    {
        var input = new EditShortcutInput(Shortcut, _id);
        var result = _viewFactory.ShowDialog<EditShortcutView, EditShortcutInput, Shortcut>(input);

        if (result is not { Success: true, Data: { } newShortcut } || Shortcut == newShortcut)
            return;

        _shortcutsService.EditShortcut(_id, newShortcut);
        Shortcut = newShortcut;
    }
}
