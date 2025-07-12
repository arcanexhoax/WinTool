using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
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

    public ShortcutsViewModel(WritableOptions<ShortcutsOptions> shortcutsOptions, KeyInterceptor keyInterceptor, EditShortcutViewModel editShortcutViewModel)
    {
        CreateFileViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.CreateFile, shortcutsOptions, editShortcutViewModel, Resources.CreateFile);
        FastFileCreationViewModel = new FastFileCreationShortcutViewModel(() => shortcutsOptions.Value.FastFileCreation, shortcutsOptions, editShortcutViewModel, Resources.FastFileCreation);
        SelectedItemCopyPathViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.SelectedItemCopyPath, shortcutsOptions, editShortcutViewModel, Resources.SelectedItemCopyPath);
        SelectedItemCopyNameViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.SelectedItemCopyName, shortcutsOptions, editShortcutViewModel, Resources.SelectedItemCopyName);
        RunWithArgsViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.RunWithArgs, shortcutsOptions, editShortcutViewModel, Resources.RunWithArgs);
        OpenFolderInCmdViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.OpenFolderInCmd, shortcutsOptions, editShortcutViewModel , Resources.OpenFolderInCmd);
        ChangeFilePropertiesViewModel = new ShortcutViewModel(() => shortcutsOptions.Value.ChangeFileProperties, shortcutsOptions, editShortcutViewModel, Resources.ChangeFileProperties);
    }
}
