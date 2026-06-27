using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System.Windows;
using System.Windows.Interop;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class EditShortcutView : ContentDialogView<EditShortcutInput, Shortcut>
{
    private readonly IKeyInterceptor _keyInterceptor;

    private nint _windowHandle;

    public EditShortcutView(EditShortcutViewModel vm, IKeyInterceptor keyInterceptor)
    {
        DataContext = vm;
        InitializeComponent();

        _keyInterceptor = keyInterceptor;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        _windowHandle = window is null ? nint.Zero : new WindowInteropHelper(window).Handle;

        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        _windowHandle = nint.Zero;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_windowHandle != nint.Zero && _windowHandle == NativeMethods.GetForegroundWindow())
        {
            if (!e.Shortcut.Key.IsModifier)
                e.IsHandled = true;

            (DataContext as EditShortcutViewModel)!.Shortcut = e.Shortcut;
        }
    }
}
