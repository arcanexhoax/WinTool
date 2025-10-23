using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyInterceptor;
using System.Collections.ObjectModel;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;
using WinTool.UI;

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
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CreateFileInteractive, shortcutContext, ShortcutNames.CreateFile, Icons.KnowledgeArticle, Resources.CreateFile),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CreateFileFast, shortcutContext, ShortcutNames.FastFileCreation, Icons.Page, Resources.FastFileCreation),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CopyFilePath, shortcutContext, ShortcutNames.SelectedItemCopyPath, Icons.Copy, Resources.SelectedItemCopyPath),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.CopyFileName, shortcutContext, ShortcutNames.SelectedItemCopyName, Icons.Rename, Resources.SelectedItemCopyName),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.RunWithArgs, shortcutContext, ShortcutNames.RunWithArgs, Icons.OpenFile, Resources.RunWithArgs),
        new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.OpenInCmd, shortcutContext, ShortcutNames.OpenFolderInCmd, Icons.CommandPrompt, Resources.OpenFolderInCmd) { IsLast = true },
        //new ShortcutViewModel(options, viewFactory, keyInterceptor, shell, ch.ChangeFileProperties, shortcutContext, ShortcutNames.ChangeFileProperties, Resources.ChangeFileProperties) { IsLast = true }
    ];
}
