using GlobalKeyInterceptor;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.UI;
using WinTool.Utils;

namespace WinTool.Services;

public partial class ShortcutsService : BackgroundService
{
    private readonly WritableOptions<ShortcutsOptions> _options;
    private readonly ShellCommandHandler _shellCommandHandler;
    private readonly ShortcutContext _shortcutContext;
    private readonly Shell _shell;
    private readonly KeyInterceptor _keyInterceptor;

    public Dictionary<Shortcut, ShortcutCommand> Shortcuts { get; } = [];

    public ShortcutsService(
        WritableOptions<ShortcutsOptions> options,
        ShellCommandHandler shellCommandHandler,
        ShortcutContext shortcutContext,
        Shell shell,
        KeyInterceptor keyInterceptor)
    {
        _options = options;
        _shellCommandHandler = shellCommandHandler;
        _shortcutContext = shortcutContext;
        _shell = shell;
        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var shortcutMatches = new (string, Action, string, string)[]
        {
            (ShortcutNames.CreateFile, async () => await _shellCommandHandler.CreateFileInteractive(), Icons.KnowledgeArticle, Resources.CreateFile),
            (ShortcutNames.FastFileCreation, _shellCommandHandler.CreateFileFast, Icons.Page, Resources.FastFileCreation),
            (ShortcutNames.SelectedItemCopyPath, _shellCommandHandler.CopyFilePath, Icons.Copy, Resources.SelectedItemCopyPath),
            (ShortcutNames.SelectedItemCopyName, _shellCommandHandler.CopyFileName, Icons.Rename, Resources.SelectedItemCopyName),
            (ShortcutNames.RunWithArgs, async () => await _shellCommandHandler.RunWithArgs(), Icons.OpenFile, Resources.RunWithArgs),
            (ShortcutNames.OpenFolderInCmd, _shellCommandHandler.OpenInCmd, Icons.CommandPrompt, Resources.OpenFolderInCmd)
        };

        foreach (var (name, command, icon, description) in shortcutMatches)
        {
            var shortcut = ShortcutUtils.Parse(_options.CurrentValue.Shortcuts[name], KeyState.Down)!;
            var shortcutCommand = new ShortcutCommand(name, shortcut, command, icon, description);
            Shortcuts[shortcut] = shortcutCommand;
        }

        return Task.CompletedTask;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_shortcutContext.IsEditing || !_shell.IsActive || !Shortcuts.TryGetValue(e.Shortcut, out var shortcutCommand))
        {
            e.IsHandled = false;
            return;
        }

        Debug.WriteLine($"Executing {shortcutCommand.Id}");

        try
        {
            shortcutCommand.Command();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to execute command {shortcutCommand.Id}: {ex.Message}");
            // TODO fix: message box is minimized
            MessageBoxHelper.ShowError(string.Format(Resources.CommandExecutionError, shortcutCommand.Id, ex.Message));
        }

        e.IsHandled = true;
        return;
    }

    public void EditShortcut(string shortcutId, Shortcut newShortcut)
    {
        var (shortcut, sc) = Shortcuts.FirstOrDefault(s => s.Value.Id == shortcutId);

        if (sc is null || shortcut is null)
            return;

        sc.Shortcut = newShortcut;

        Shortcuts.Remove(shortcut);
        Shortcuts[newShortcut] = sc;

        _options.Update(o => o.Shortcuts[sc.Id] = sc.Shortcut.ToString());
    }
}
