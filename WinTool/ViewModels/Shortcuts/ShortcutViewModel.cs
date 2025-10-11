using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using WinTool.Options;
using WinTool.Services;
using WinTool.Utils;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;

    public Shortcut? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        WindowFactory windowFactory,
        string shortcutName,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;

        Shortcut = ShortcutUtils.Parse(_shortcutsOptions.CurrentValue.Shortcuts[shortcutName], KeyState.Down);
        Description = description;

        EditShortcutCommand = new RelayCommand(() =>
        {   
            var window = windowFactory.Create<EditShortcutWindow>();
            window.ShowDialog(Shortcut);
        });
    }
}
