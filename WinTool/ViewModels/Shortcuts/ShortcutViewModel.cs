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
    private readonly KeyInterceptor _keyInterceptor;
    private readonly Shell _shell;
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
        _keyInterceptor = keyInterceptor;
        _shell = shell;
        _shortcutName = shortcutName;

        Shortcut = ShortcutUtils.Parse(_shortcutsOptions.CurrentValue.Shortcuts[shortcutName], KeyState.Down);
        Description = description;

        _keyInterceptor.RegisterShortcut(Shortcut, () => ExecuteCommand(command));

        EditShortcutCommand = new RelayCommand(() =>
        {   
            var window = windowFactory.Create<EditShortcutWindow>();
            window.ShowDialog(Shortcut);
        });
    }

    private bool ExecuteCommand(Action command)
    {
        Debug.WriteLine($"Executing {_shortcutName}");
        var shellActive = _shell.IsActive;

        // use a new thread because it is unable to get shell windows from MTA thread
        Thread t = new(() =>
        {
            try
            {
                command();
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
}
