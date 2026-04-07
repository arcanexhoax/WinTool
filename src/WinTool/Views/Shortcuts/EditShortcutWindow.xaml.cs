using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class EditShortcutWindow : DialogWindow<EditShortcutInput, Shortcut>
{
    private readonly IKeyInterceptor _keyInterceptor;

    public EditShortcutWindow(EditShortcutViewModel vm, IKeyInterceptor keyInterceptor)
    {
        DataContext = vm;
        InitializeComponent();

        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
    }

    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (_handle == NativeMethods.GetForegroundWindow())
        {
            if (!e.Shortcut.Key.IsModifier)
                e.IsHandled = true;

            (DataContext as EditShortcutViewModel)!.Shortcut = e.Shortcut;
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        base.OnClosed(e);
    }
}
