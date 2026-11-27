using GlobalKeyInterceptor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinTool.WinUI.Models;
using WinTool.WinUI.Options;

namespace WinTool.WinUI.Services;

public partial class ShortcutsService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IKeyInterceptor _keyInterceptor;
    private readonly WritableOptions<ShortcutsOptions> _options;
    private readonly ShellCommandHandler _shellCommandHandler;
    private readonly ShortcutContext _shortcutContext;
    private readonly Shell _shell;

    public Dictionary<string, ShortcutCommand> AvailableCommands { get; }
    public Dictionary<Shortcut, ShortcutCommand> Shortcuts { get; } = [];

    public ShortcutsService(
        ILogger<ShortcutsService> logger,
        IKeyInterceptor keyInterceptor,
        WritableOptions<ShortcutsOptions> options,
        ShellCommandHandler shellCommandHandler,
        ShortcutContext shortcutContext,
        Shell shell)
    {
        _logger = logger;
        _options = options;
        _shellCommandHandler = shellCommandHandler;
        _shortcutContext = shortcutContext;
        _shell = shell;
        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;

        AvailableCommands = new Dictionary<string, ShortcutCommand>()
        {
            { ShortcutNames.CreateFile, new(ShortcutNames.CreateFile, null, _shellCommandHandler.CreateFileAsync) },
            { ShortcutNames.SelectedItemCopyPath, new(ShortcutNames.SelectedItemCopyPath, null, _shellCommandHandler.CopyFilePathAsync) },
            { ShortcutNames.SelectedItemCopyName, new(ShortcutNames.SelectedItemCopyName, null, _shellCommandHandler.CopyFileNameAsync) },
            { ShortcutNames.RunFileAsAdmin, new(ShortcutNames.RunFileAsAdmin, null, _shellCommandHandler.RunFileAsAdminAsync) },
            { ShortcutNames.RunFileWithArgs, new(ShortcutNames.RunFileWithArgs, null, _shellCommandHandler.RunFileWithArgsAsync) },
            { ShortcutNames.OpenFolderInCmd, new(ShortcutNames.OpenFolderInCmd, null, _shellCommandHandler.OpenInCmdAsync) },
            { ShortcutNames.OpenFolderInCmdAsAdmin, new(ShortcutNames.OpenFolderInCmdAsAdmin, null, _shellCommandHandler.OpenInCmdAsAdminAsync) }
        };
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO move to ctor
        foreach (var (name, scStr) in _options.CurrentValue.Shortcuts)
        {
            if (Shortcut.TryParse(scStr, KeyState.Down, out var shortcut)
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

        Task.Run(async () =>
        {
            _logger.LogDebug("Executing shortcut command {CommandId}", shortcutCommand.Id);

            try
            {
                await shortcutCommand.Command();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command {CommandId}", shortcutCommand.Id);
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
        Shortcuts[newShortcut] = shortcutCommand;

        _options.Update(o => o.Shortcuts[shortcutCommand.Id] = shortcutCommand.Shortcut.ToString());
        _logger.LogInformation("Shortcut for command {CommandId} updated to {Shortcut}", shortcutCommand.Id, newShortcut);
    }
}
