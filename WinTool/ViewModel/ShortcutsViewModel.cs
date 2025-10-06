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
        CreateFileViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.CreateFile, shortcutsOptions, editShortcutViewModel, Resources.CreateFile);
        FastFileCreationViewModel = new FastFileCreationShortcutViewModel(() => shortcutsOptions.CurrentValue.FastFileCreation, shortcutsOptions, editShortcutViewModel, Resources.FastFileCreation);
        SelectedItemCopyPathViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.SelectedItemCopyPath, shortcutsOptions, editShortcutViewModel, Resources.SelectedItemCopyPath);
        SelectedItemCopyNameViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.SelectedItemCopyName, shortcutsOptions, editShortcutViewModel, Resources.SelectedItemCopyName);
        RunWithArgsViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.RunWithArgs, shortcutsOptions, editShortcutViewModel, Resources.RunWithArgs);
        OpenFolderInCmdViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.OpenFolderInCmd, shortcutsOptions, editShortcutViewModel , Resources.OpenFolderInCmd);
        ChangeFilePropertiesViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.ChangeFileProperties, shortcutsOptions, editShortcutViewModel, Resources.ChangeFileProperties);
    }
}
