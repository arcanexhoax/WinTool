using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModel;

namespace WinTool.Views;

public enum WindowBackdropType
{
    None,
    Auto,
    Mica,
    Acrylic,
    Tabbed
}

public class FluentWindow : Window
{
    public FluentWindow()
    {
        SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(this).Handle;
            NativeMethods.RemoveTitlebarBackground(handle);
        };
    }

    protected void ApplyBackdrop(nint handle, WindowBackdropType backdropType)
    {
        var pvAttr = 1u;
        var dwmWinAttr = DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT;

        // 22H1
        if (Environment.OSVersion.Version.Build < 22053)
        {
            if (backdropType != WindowBackdropType.None)
                ApplyBackdrop(pvAttr, handle, dwmWinAttr);

            return;
        }

        dwmWinAttr = DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE;
        pvAttr = (uint)(backdropType switch
        {
            WindowBackdropType.Auto => DWMSBT.AUTO,
            WindowBackdropType.Mica => DWMSBT.MAINWINDOW,
            WindowBackdropType.Acrylic => DWMSBT.TRANSIENTWINDOW,
            WindowBackdropType.Tabbed => DWMSBT.TABBEDWINDOW,
            _ => DWMSBT.DISABLE,        
        });

        ApplyBackdrop(pvAttr, handle, dwmWinAttr);
    }

    private void ApplyBackdrop(uint pvAttr, nint handle, DWMWINDOWATTRIBUTE dwmAttr)
    {
        NativeMethods.DwmSetWindowAttribute(handle, dwmAttr, ref pvAttr, Marshal.SizeOf<int>());
    }
}

public class ModalWindow : FluentWindow
{
    public ModalWindow()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BringProcessToForeground();
        Loaded -= OnLoaded;
    }

    private void BringProcessToForeground()
    {
        INPUT input = new()
        {
            Type = INPUTTYPE.INPUTMOUSE,
            Data = { }
        };
        INPUT[] inputs = [input];

        // Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
        _ = NativeMethods.SendInput(1, inputs, INPUT.Size);
        Activate();
    }
}

public class DialogWindow<TIn, TOut> : ModalWindow
{
    public Result<TOut> ShowDialog(TIn data)
    {
        var vm = (IDialogViewModel<TIn, TOut>)DataContext;
        Result<TOut>? result = null;

        SourceInitialized += (_, _) =>
        {
            vm.OnShow(data, r =>
            {
                result = r;
                Close();
            });
        };

        ShowDialog();
        vm.OnClose();

        return result ?? new Result<TOut>(false);
    }
}
