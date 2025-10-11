using GlobalKeyInterceptor;
using System.Diagnostics;
using System.Windows.Interop;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class EditShortcutWindow : DialogWindow<EditShortcutInput, Shortcut>
{
    private readonly KeyInterceptor _keyInterceptor;

    private nint _handle;

    public EditShortcutWindow(EditShortcutViewModel vm, KeyInterceptor keyInterceptor)
    {
        DataContext = vm;
        InitializeComponent();

        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;

        SourceInitialized += (_, _) => _handle = new WindowInteropHelper(this).Handle;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_handle == NativeMethods.GetForegroundWindow())
        {
            Debug.WriteLine(e.Shortcut);
            (DataContext as EditShortcutViewModel)!.Shortcut = e.Shortcut;
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        base.OnClosed(e);
    }
}
