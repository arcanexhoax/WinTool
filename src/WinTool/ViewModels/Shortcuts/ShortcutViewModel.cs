using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using System.Diagnostics;
using System.Threading;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutViewModel : ObservableObject
{
    private readonly string _id;
    private readonly ShortcutsService _shortcutsService;
    private readonly ViewFactory _viewFactory;

    public Shortcut? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public bool IsLast
    {
        get; set => SetProperty(ref field, value);
    }

    public string Icon { get; }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        ShortcutsService shortcutsService,
        ShortcutCommand shortcutCommand,
        ViewFactory viewFactory)
    {
        _shortcutsService = shortcutsService;
        _viewFactory = viewFactory;
        _id = shortcutCommand.Id;

        Shortcut = shortcutCommand.Shortcut;
        Icon = shortcutCommand.Icon;
        Description = shortcutCommand.Description;

        EditShortcutCommand = new RelayCommand(Edit);
    }

    private void Edit()
    {
        if (Shortcut is null)
            return;

        var window = _viewFactory.Create<EditShortcutWindow>();
        var result = window.ShowDialog(new EditShortcutInput(Shortcut, _id));

        if (result is not { Success: true, Data: { } newShortcut } || Shortcut == newShortcut)
            return;

        _shortcutsService.EditShortcut(_id, newShortcut);
        Shortcut = newShortcut;
    }
}
