using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
using System.Collections.ObjectModel;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutsViewModel(
    WritableOptions<ShortcutsOptions> options,
    WindowFactory windowFactory,
    KeyInterceptor keyInterceptor,
    Shell shell,
    ShellCommandHandler ch,
    ShortcutContext shortcutContext) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } =
    [
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CreateFileInteractive, shortcutContext, ShortcutNames.CreateFile, Resources.CreateFile),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CreateFileFast, shortcutContext, ShortcutNames.FastFileCreation,  Resources.FastFileCreation),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CopyFilePath, shortcutContext, ShortcutNames.SelectedItemCopyPath, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.CopyFileName, shortcutContext, ShortcutNames.SelectedItemCopyName, Resources.SelectedItemCopyName),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.RunWithArgs, shortcutContext, ShortcutNames.RunWithArgs, Resources.RunWithArgs),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.OpenInCmd, shortcutContext, ShortcutNames.OpenFolderInCmd, Resources.OpenFolderInCmd),
        new ShortcutViewModel(options, windowFactory, keyInterceptor, shell, ch.ChangeFileProperties, shortcutContext, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties)
    ];
}
