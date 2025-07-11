using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Options;
using WinTool.Properties;

namespace WinTool.ViewModel;

public class ShortcutsViewModel : ObservableObject
{
    public ShortcutViewModel CreateFileViewModel { get; }
    public FastFileCreationShortcutViewModel FastFileCreationViewModel { get; }
    public ShortcutViewModel SelectedItemCopyPathViewModel { get; }
    public ShortcutViewModel SelectedItemCopyNameViewModel { get; }
    public ShortcutViewModel RunWithArgsViewModel { get; }
    public ShortcutViewModel OpenFolderInCmdViewModel { get; }
    public ShortcutViewModel ChangeFilePropertiesViewModel { get; }

    public ShortcutsViewModel(WritableOptions<ShortcutsOptions> shortcutsOptions)
    {
        CreateFileViewModel = new ShortcutViewModel(shortcutsOptions.Value.CreateFile, shortcutsOptions, Resources.CreateFile);
        FastFileCreationViewModel = new FastFileCreationShortcutViewModel(shortcutsOptions.Value.FastFileCreation, shortcutsOptions, Resources.FastFileCreation);
        SelectedItemCopyPathViewModel = new ShortcutViewModel(shortcutsOptions.Value.SelectedItemCopyPath, shortcutsOptions, Resources.SelectedItemCopyPath);
        SelectedItemCopyNameViewModel = new ShortcutViewModel(shortcutsOptions.Value.SelectedItemCopyName, shortcutsOptions, Resources.SelectedItemCopyName);
        RunWithArgsViewModel = new ShortcutViewModel(shortcutsOptions.Value.RunWithArgs, shortcutsOptions, Resources.RunWithArgs);
        OpenFolderInCmdViewModel = new ShortcutViewModel(shortcutsOptions.Value.OpenFolderInCmd, shortcutsOptions, Resources.OpenFolderInCmd);
        ChangeFilePropertiesViewModel = new ShortcutViewModel(shortcutsOptions.Value.ChangeFileProperties, shortcutsOptions, Resources.ChangeFileProperties);
    }
}
