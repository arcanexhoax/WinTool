using GlobalKeyInterceptor;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.UI;

namespace WinTool.Services;

public class ShortcutsService : BackgroundService
{
    private readonly WritableOptions<ShortcutsOptions> _options;
    private readonly ShellCommandHandler _shellCommandHandler;
    private readonly ShortcutContext _shortcutContext;
    private readonly Shell _shell;
    private readonly KeyInterceptor _keyInterceptor;

    public Dictionary<string, ShortcutCommand> AvailableCommands { get; }
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

        AvailableCommands = new Dictionary<string, ShortcutCommand>()
        {
            { ShortcutNames.CreateFile, new(ShortcutNames.CreateFile, null, _shellCommandHandler.CreateFileInteractive, Icons.KnowledgeArticle, Resources.CreateFile) },
            { ShortcutNames.FastFileCreation, new(ShortcutNames.FastFileCreation, null, _shellCommandHandler.CreateFileFast, Icons.Page, Resources.FastFileCreation) },
            { ShortcutNames.SelectedItemCopyPath, new(ShortcutNames.SelectedItemCopyPath, null, _shellCommandHandler.CopyFilePath, Icons.Copy, Resources.SelectedItemCopyPath) },
            { ShortcutNames.SelectedItemCopyName, new(ShortcutNames.SelectedItemCopyName, null, _shellCommandHandler.CopyFileName, Icons.Rename, Resources.SelectedItemCopyName) },
            { ShortcutNames.RunFileAsAdmin, new(ShortcutNames.RunFileAsAdmin, null, _shellCommandHandler.RunFileAsAdmin, Icons.ProtectedDocument, Resources.RunFileAsAdmin) },
            { ShortcutNames.RunFileWithArgs, new(ShortcutNames.RunFileWithArgs, null, _shellCommandHandler.RunFileWithArgs, Icons.OpenFile, Resources.RunFileWithArgs) },
            { ShortcutNames.OpenFolderInCmd, new(ShortcutNames.OpenFolderInCmd, null, _shellCommandHandler.OpenInCmd, Icons.CommandPrompt, Resources.OpenFolderInCmd) },
            { ShortcutNames.OpenFolderInCmdAsAdmin, new(ShortcutNames.OpenFolderInCmdAsAdmin, null, _shellCommandHandler.OpenInCmdAsAdmin, Icons.CommandPrompt, Resources.OpenFolderInCmdAsAdmin) }
        };
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var (name, scStr) in _options.CurrentValue.Shortcuts)
        {
            if (Shortcut.Parse(scStr) is { } shortcut 
                && AvailableCommands.TryGetValue(name, out var shortcutCommand))
            {
                shortcutCommand.Shortcut = shortcut;
                Shortcuts[shortcut] = shortcutCommand;
            }
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

        Task.Run(() =>
        {
            Debug.WriteLine($"Executing {shortcutCommand.Id}");

            try
            {
                shortcutCommand.Command();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to execute command {shortcutCommand.Id}: {ex.Message}");
                // TODO fix: message box is minimized
                MessageBox.ShowError(string.Format(Resources.CommandExecutionError, shortcutCommand.Id, ex.Message));
            }
        });

        e.IsHandled = true;
    }

    public void EditShortcut(string shortcutId, Shortcut newShortcut)
    {
        if (!AvailableCommands.TryGetValue(shortcutId, out var shortcutCommand))
            return;

        if (shortcutCommand.Shortcut != null)
            Shortcuts.Remove(shortcutCommand.Shortcut);

        shortcutCommand.Shortcut = newShortcut;
        Shortcuts[newShortcut]= shortcutCommand;

        _options.Update(o => o.Shortcuts[shortcutCommand.Id] = shortcutCommand.Shortcut.ToString());
    }
}
