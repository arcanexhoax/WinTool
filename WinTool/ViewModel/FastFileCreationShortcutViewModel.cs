using GlobalKeyInterceptor;
using System;
using WinTool.Options;

namespace WinTool.ViewModel;

public class FastFileCreationShortcutViewModel : ShortcutViewModel
{
    public string NewFileTemplate
    {
        get; set
        {
            if (value is not null && SetProperty(ref field, value) && _shortcutsOptions.CurrentValue.FastFileCreation.NewFileTemplate != value)
            {
                _shortcutsOptions.Update(() => _shortcutsOptions.CurrentValue.FastFileCreation.NewFileTemplate = value);
            }
        }
    }

    public FastFileCreationShortcutViewModel(
        Func<FastFileCreationShortcutOptions> optionsFactory,
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        EditShortcutViewModel editShortcutViewModel,
        KeyInterceptor keyInterceptor,
        string description) : base(optionsFactory, shortcutsOptions, editShortcutViewModel, keyInterceptor, description)
    {
        NewFileTemplate = optionsFactory().NewFileTemplate;
    }
}
