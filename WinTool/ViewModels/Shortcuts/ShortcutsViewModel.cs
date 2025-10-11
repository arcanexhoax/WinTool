using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
using System.Collections.ObjectModel;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutsViewModel(WritableOptions<ShortcutsOptions> options, WindowFactory windowFactory, KeyInterceptor keyInterceptor, Shell shell, ShellCommandHandler ch) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } =
    [
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CreateFileInteractive, ShortcutNames.CreateFile, Resources.CreateFile),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CreateFileFast, ShortcutNames.FastFileCreation,  Resources.FastFileCreation),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CopyFilePath, ShortcutNames.SelectedItemCopyPath, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CopyFileName, ShortcutNames.SelectedItemCopyName, Resources.SelectedItemCopyName),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.RunWithArgs, ShortcutNames.RunWithArgs, Resources.RunWithArgs),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.OpenInCmd, ShortcutNames.OpenFolderInCmd, Resources.OpenFolderInCmd),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.ChangeFileProperties, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties)
    ];
}
