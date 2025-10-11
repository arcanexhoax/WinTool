using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using WinTool.Options;
using WinTool.Utils;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

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
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        EditShortcutViewModel editShortcutViewModel,
        KeyInterceptor keyInterceptor,
        string shortcutName,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;
        _editShortcutViewModel = editShortcutViewModel;

        Shortcut = ShortcutUtils.Parse(_shortcutsOptions.CurrentValue.Shortcuts[shortcutName], KeyState.Down);
        Description = description;

        EditShortcutCommand = new RelayCommand(() =>
        {   
            var window = new EditShortcutWindow(_editShortcutViewModel, keyInterceptor);
            window.ShowDialog(Shortcut);
        });
    }
}
