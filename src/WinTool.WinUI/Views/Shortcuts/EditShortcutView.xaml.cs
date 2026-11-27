using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using Microsoft.UI.Xaml.Controls;
using System;
using WinRT.Interop;
using WinTool.WinUI.Native;
using WinTool.WinUI.ViewModels.Shortcuts;

namespace WinTool.WinUI.Views.Shortcuts;

public partial class EditShortcutView : ContentDialog
{
    private readonly IKeyInterceptor _keyInterceptor;

    private nint _windowHandle;

    public EditShortcutViewModel ViewModel { get; }

    public EditShortcutView(EditShortcutViewModel vm, IKeyInterceptor keyInterceptor)
    {
        ViewModel = vm;
        _keyInterceptor = keyInterceptor;

        InitializeComponent();

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        _windowHandle = App.MainWindow is { } window ? WindowNative.GetWindowHandle(window) : nint.Zero;

        ViewModel.SaveCommand.CanExecuteChanged += OnSaveCanExecuteChanged;
        OnSaveCanExecuteChanged(this, EventArgs.Empty);

        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        ViewModel.SaveCommand.CanExecuteChanged -= OnSaveCanExecuteChanged;
        _windowHandle = nint.Zero;
    }

    private void OnSaveCanExecuteChanged(object? sender, EventArgs e) => IsPrimaryButtonEnabled = ViewModel.SaveCommand.CanExecute(null);

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_windowHandle != nint.Zero && _windowHandle == NativeMethods.GetForegroundWindow())
        {
            if (!e.Shortcut.Key.IsModifier)
                e.IsHandled = true;

            DispatcherQueue.TryEnqueue(() => ViewModel.Shortcut = e.Shortcut);
        }
    }
}
