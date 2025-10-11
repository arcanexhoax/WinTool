using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
using System.Collections.ObjectModel;
using WinTool.Options;
using WinTool.Properties;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutsViewModel(WritableOptions<ShortcutsOptions> shortcutsOptions, KeyInterceptor keyInterceptor, EditShortcutViewModel editShortcutViewModel) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } =
    [
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.CreateFile, Resources.CreateFile),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.FastFileCreation,  Resources.FastFileCreation),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.SelectedItemCopyPath, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.SelectedItemCopyName, Resources.SelectedItemCopyName),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.RunWithArgs, Resources.RunWithArgs),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.OpenFolderInCmd, Resources.OpenFolderInCmd),
        new ShortcutViewModel(shortcutsOptions, editShortcutViewModel, keyInterceptor, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties)
    ];
}
