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
    ViewFactory viewFactory,
    KeyInterceptor keyInterceptor,
    Shell shell,
    ShellCommandHandler ch,
    ShortcutContext shortcutContext) : ObservableObject
{
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } =
    [
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CreateFileInteractive, shortcutContext, ShortcutNames.CreateFile, Resources.CreateFile),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CreateFileFast, shortcutContext, ShortcutNames.FastFileCreation,  Resources.FastFileCreation),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CopyFilePath, shortcutContext, ShortcutNames.SelectedItemCopyPath, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CopyFileName, shortcutContext, ShortcutNames.SelectedItemCopyName, Resources.SelectedItemCopyName),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.RunWithArgs, shortcutContext, ShortcutNames.RunWithArgs, Resources.RunWithArgs),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.OpenInCmd, shortcutContext, ShortcutNames.OpenFolderInCmd, Resources.OpenFolderInCmd) { IsLast = true },
        //new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.ChangeFileProperties, shortcutContext, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties) { IsLast = true }
    ];
}
