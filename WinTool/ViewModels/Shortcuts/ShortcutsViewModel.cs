using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutsViewModel(WritableOptions<ShortcutsOptions> shortcutsOptions, WindowFactory windowFactory) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } =
    [
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.CreateFile, Resources.CreateFile),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.FastFileCreation,  Resources.FastFileCreation),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.SelectedItemCopyPath, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.SelectedItemCopyName, Resources.SelectedItemCopyName),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.RunWithArgs, Resources.RunWithArgs),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.OpenFolderInCmd, Resources.OpenFolderInCmd),
        new ShortcutViewModel(shortcutsOptions, windowFactory, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties)
    ];
}
