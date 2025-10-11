using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using System.Diagnostics;
using System.Threading;
using WinTool.Options;
using WinTool.Properties;
using WinTool.Services;
using WinTool.Utils;
using WinTool.Views.Shortcuts;

namespace WinTool.ViewModels.Shortcuts;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;
    private readonly WindowFactory _windowFactory;
    private readonly KeyInterceptor _keyInterceptor;
    private readonly Shell _shell;
    private readonly Action _command;
    private readonly string _shortcutName;

    public Shortcut? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        WindowFactory windowFactory,
        KeyInterceptor keyInterceptor,
        Shell shell,
        Action command,
        string shortcutName,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;
        _windowFactory = windowFactory;
        _keyInterceptor = keyInterceptor;
        _shell = shell;
        _command = command;
        _shortcutName = shortcutName;

        Shortcut = ShortcutUtils.Parse(_shortcutsOptions.CurrentValue.Shortcuts[shortcutName], KeyState.Down);
        Description = description;

        _keyInterceptor.RegisterShortcut(Shortcut, ExecuteCommand);

        EditShortcutCommand = new RelayCommand(Edit);
    }

    private bool ExecuteCommand()
    {
        Debug.WriteLine($"Executing {_shortcutName}");
        var shellActive = _shell.IsActive;

        // use a new thread because it is unable to get shell windows from MTA thread
        Thread t = new(() =>
        {
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
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        return shellActive;
    }

    private void Edit()
    {
        var window = _windowFactory.Create<EditShortcutWindow>();
        var result = window.ShowDialog(Shortcut);

        if (result is not { Success: true, Data: { } newShortcut } || Shortcut == newShortcut)
            return;

        _keyInterceptor.UnregisterShortcut(Shortcut, ExecuteCommand);
        Shortcut = newShortcut;
        _keyInterceptor.RegisterShortcut(Shortcut, ExecuteCommand);

        _shortcutsOptions.Update(o => o.Shortcuts[_shortcutName] = Shortcut.ToString());
    }
}
