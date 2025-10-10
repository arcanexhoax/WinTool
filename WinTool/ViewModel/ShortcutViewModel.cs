using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using WinTool.Options;
using WinTool.Utils;
using WinTool.View;

namespace WinTool.ViewModel;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;
    protected readonly EditShortcutViewModel _editShortcutViewModel;

    public Shortcut? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        Func<ShortcutOptions> optionsFactory,
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        EditShortcutViewModel editShortcutViewModel,
        KeyInterceptor keyInterceptor,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;
        _editShortcutViewModel = editShortcutViewModel;

        Shortcut = ShortcutUtils.Parse(optionsFactory().Shortcut, KeyState.Down);
        Description = description;

        // TODO move fast file creation shortcut view into shortcut view
        EditShortcutCommand = new RelayCommand(() =>
        {   
            var window = new EditShortcutWindow(_editShortcutViewModel, keyInterceptor);
            window.ShowDialog(Shortcut);
        });
    }
}
