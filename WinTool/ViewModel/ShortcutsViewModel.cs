using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Options;

namespace WinTool.ViewModel;

public class ShortcutsViewModel : ObservableObject
{
    private readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;

    public string NewFileTemplate
    {
        get; set
        {
            if (value is not null && SetProperty(ref field, value))
            {
                _shortcutsOptions.Value.FastFileCreation.NewFileTemplate = value;
                _shortcutsOptions.Update();
            }
        }
    }

    public ShortcutsViewModel(WritableOptions<ShortcutsOptions> shortcutsOptions)
    {
        _shortcutsOptions = shortcutsOptions;
        NewFileTemplate = _shortcutsOptions.Value.FastFileCreation.NewFileTemplate;
    }
}
