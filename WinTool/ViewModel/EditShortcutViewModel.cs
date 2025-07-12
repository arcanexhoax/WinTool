using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Text;
using WinTool.Model;
using WinTool.Native;

namespace WinTool.ViewModel;

public class EditShortcutViewModel : ObservableObject
{
    private readonly KeyInterceptor _keyInterceptor;

    private Shortcut? _currentShortcut;
    private Action<Result<string>>? _onResult;
    private nint _handle;

    public string? Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public EditShortcutViewModel(KeyInterceptor keyInterceptor)
    {
        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;

        SaveCommand = new RelayCommand(() =>
        {
            if (_currentShortcut is null)
            {
                _onResult?.Invoke(new Result<string>(false));
                return;
            }

            if (_currentShortcut.Modifier is KeyModifier.None)
                return;

            _onResult?.Invoke(new Result<string>(true, Shortcut));
        });
        CancelCommand = new RelayCommand(() => _onResult?.Invoke(new Result<string>(false)));
    }

    // TODO add this approach to another modal windows
    public void StartEditing(nint handle, string shortcut, Action<Result<string>> onResult)
    {
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
        _handle = handle;
        _onResult = onResult;

        Shortcut = shortcut;
    }

    public void StopEditing()
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        _onResult = null;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_handle != NativeMethods.GetForegroundWindow())
            return;

        if (e.Shortcut.State is KeyState.Down)
        {
            _currentShortcut = e.Shortcut;
            Shortcut = _currentShortcut.ToString();
        }
    }
}
