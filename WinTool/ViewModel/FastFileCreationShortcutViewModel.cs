using WinTool.Options;

namespace WinTool.ViewModel;

public class FastFileCreationShortcutViewModel : ShortcutViewModel
{
    public string NewFileTemplate
    {
        get; set
        {
            if (value is not null && SetProperty(ref field, value) && _shortcutsOptions.Value.FastFileCreation.NewFileTemplate != value)
            {
                _shortcutsOptions.Value.FastFileCreation.NewFileTemplate = value;
                _shortcutsOptions.Update();
            }
        }
    }

    public FastFileCreationShortcutViewModel(
        FastFileCreationShortcutOptions options,
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        string description) : base(options, shortcutsOptions, description)
    {
        NewFileTemplate = options.NewFileTemplate;
    }
}
