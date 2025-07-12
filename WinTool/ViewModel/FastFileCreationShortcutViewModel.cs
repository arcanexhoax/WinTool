using System;
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
        Func<FastFileCreationShortcutOptions> optionsFactory,
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        EditShortcutViewModel editShortcutViewModel,
        string description) : base(optionsFactory, shortcutsOptions, editShortcutViewModel, description)
    {
        NewFileTemplate = optionsFactory().NewFileTemplate;
    }
}
