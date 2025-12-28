using GlobalKeyInterceptor;
using System.Diagnostics;
using WinRT.Interop;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public class EditShortcutWindowBase : DialogWindow<EditShortcutInput, Shortcut> { }

public sealed partial class EditShortcutWindow : EditShortcutWindowBase
{
    private readonly KeyInterceptor _keyInterceptor;
    private readonly nint _handle;

    public EditShortcutWindow(EditShortcutViewModel vm, KeyInterceptor keyInterceptor)
    {
        InitializeComponent();
        DataContext = vm; 

        _handle = WindowNative.GetWindowHandle(this);
        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;

        Closed += OnClosed;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_handle == NativeMethods.GetForegroundWindow())
        {
            Debug.WriteLine(e.Shortcut);
            (DataContext as EditShortcutViewModel)!.Shortcut = e.Shortcut;
        }
    }

    private void OnClosed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
    }
}
