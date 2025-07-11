using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Options;

namespace WinTool.ViewModel;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;

    public string Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string BackingShortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public ShortcutViewModel(ShortcutOptions options, WritableOptions<ShortcutsOptions> shortcutsOptions, string description)
    {
        _shortcutsOptions = shortcutsOptions;

        Shortcut = options.Shortcut;
        BackingShortcut = Shortcut;
        Description = description;
    }
}
