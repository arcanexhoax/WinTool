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
        CreateFileViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.CreateFile, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.CreateFile);
        FastFileCreationViewModel = new FastFileCreationShortcutViewModel(() => shortcutsOptions.CurrentValue.FastFileCreation, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.FastFileCreation);
        SelectedItemCopyPathViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.SelectedItemCopyPath, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.SelectedItemCopyPath);
        SelectedItemCopyNameViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.SelectedItemCopyName, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.SelectedItemCopyName);
        RunWithArgsViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.RunWithArgs, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.RunWithArgs);
        OpenFolderInCmdViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.OpenFolderInCmd, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.OpenFolderInCmd);
        ChangeFilePropertiesViewModel = new ShortcutViewModel(() => shortcutsOptions.CurrentValue.ChangeFileProperties, shortcutsOptions, editShortcutViewModel, keyInterceptor, Resources.ChangeFileProperties);
    }
}
