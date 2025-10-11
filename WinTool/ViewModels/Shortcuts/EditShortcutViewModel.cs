using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using System;
using WinTool.Models;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public class EditShortcutViewModel : ObservableObject, IModalViewModel<Shortcut?, Shortcut>
{
    private Action<Result<Shortcut>>? _onResult;

    public Shortcut? Shortcut
    {
        get; set
        {
            if (value?.State is KeyState.Down)
            {
                SetProperty(ref field, value);
            }
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public EditShortcutViewModel()
    {
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => _onResult?.Invoke(new Result<Shortcut>(false)));
    }

    public void OnShow(Shortcut? input, Action<Result<Shortcut>> onResult)
    {
        Shortcut = input;
        _onResult = onResult;
    }

    public void OnClose()
    {
        _onResult = null;
    }

    private void Save()
    {
        if (Shortcut is null)
        {
            _onResult?.Invoke(new Result<Shortcut>(false));
            return;
        }

        if (Shortcut.Modifier is KeyModifier.None)
            return;

        _onResult?.Invoke(new Result<Shortcut>(true, Shortcut));
    }
}
