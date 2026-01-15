using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public partial class EditShortcutViewModel : ObservableObject, IDialogViewModel<EditShortcutInput, Shortcut>
{
    private readonly IOptionsMonitor<ShortcutsOptions> _options;
    private readonly ShortcutContext _shortcutContext;

    private Action<Result<Shortcut>>? _onResult;
    private string? _name;

    public Shortcut? Shortcut
    {
        get; set
        {
            if (value is { State: KeyState.Up })
                return;
            
            field = value;
            IsErrorShown = false;
            ShortcutString = value?.ToFormattedString();
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    [ObservableProperty]
    public partial string? ShortcutString { get; set; }

    [ObservableProperty]
    public partial bool IsErrorShown { get; set; }

    [ObservableProperty]
    public partial string? ErrorText { get; set; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public EditShortcutViewModel(IOptionsMonitor<ShortcutsOptions> options, ShortcutContext shortcutContext)
    {
        _options = options;
        _shortcutContext = shortcutContext;

        SaveCommand = new RelayCommand(() => _onResult?.Invoke(new Result<Shortcut>(true, Shortcut)), IsSavable);
        CancelCommand = new RelayCommand(() => _onResult?.Invoke(new Result<Shortcut>(false)));
    }

    public void OnShow(EditShortcutInput input, Action<Result<Shortcut>> onResult)
    {
        _name = input.Name;
        _onResult = onResult;

        Shortcut = input.Shortcut;

        _shortcutContext.IsEditing = true;
    }

    public void OnClose()
    {
        _shortcutContext.IsEditing = false;
        _onResult = null;
    }

    private bool IsSavable()
    {
        if (Shortcut is not { Modifier: not KeyModifier.None })
            return false;

        var existing = _options.CurrentValue.Shortcuts.FirstOrDefault(
            s => s.Key != _name && s.Value == Shortcut.ToString(),
            new(string.Empty, string.Empty));

        if (existing.Value is not [])
        {
            ErrorText = string.Format(Resources.ShortcutAlreadyUsed, existing.Key);
            IsErrorShown = true;
            return false;
        }

        return true;
    }
}
