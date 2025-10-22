using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using System.Diagnostics;
using System.Threading;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;
    private readonly ViewFactory _viewFactory;
    private readonly KeyInterceptor _keyInterceptor;
    private readonly Shell _shell;
    private readonly Action _command;
    private readonly ShortcutContext _shortcutContext;
    private readonly string _shortcutName;

    public Shortcut? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public bool IsLast
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        ViewFactory viewFactory,
        KeyInterceptor keyInterceptor,
        Shell shell,
        Action command,
        ShortcutContext shortcutContext,
        string shortcutName,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;
        _viewFactory = viewFactory;
        _keyInterceptor = keyInterceptor;
        _shell = shell;
        _command = command;
        _shortcutContext = shortcutContext;
        _shortcutName = shortcutName;

        Shortcut = ShortcutUtils.Parse(_shortcutsOptions.CurrentValue.Shortcuts[shortcutName], KeyState.Down);
        Description = description;

        _keyInterceptor.RegisterShortcut(Shortcut, ExecuteCommand);

        EditShortcutCommand = new RelayCommand(Edit);
    }

    private bool ExecuteCommand()
    {
        if (_shortcutContext.IsEditing)
            return false;

        Debug.WriteLine($"Executing {_shortcutName}");

        if (!_shell.IsActive)
            return false;

        try
        {
            _command();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to execute command {_shortcutName}: {ex.Message}");
            // TODO fix: message box is minimized
            MessageBoxHelper.ShowError(string.Format(Resources.CommandExecutionError, _shortcutName, ex.Message));
        }

        return true;
    }

    private void Edit()
    {
        if (Shortcut is null)
            return;

        var window = _viewFactory.Create<EditShortcutWindow>();
        var result = window.ShowDialog(new EditShortcutInput(Shortcut, _shortcutName));

        if (result is not { Success: true, Data: { } newShortcut } || Shortcut == newShortcut)
            return;

        _keyInterceptor.UnregisterShortcut(Shortcut, ExecuteCommand);
        Shortcut = newShortcut;
        _keyInterceptor.RegisterShortcut(Shortcut, ExecuteCommand);

        _shortcutsOptions.Update(o => o.Shortcuts[_shortcutName] = Shortcut.ToString());
    }
}
