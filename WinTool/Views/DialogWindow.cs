using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
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
    protected nint _handle;
    protected HwndSource? _handleSource;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _handle = new WindowInteropHelper(this).Handle;
        _handleSource = HwndSource.FromHwnd(_handle);

        RemoveTitlebarBackground();
    }

    protected void ApplyBackdrop(WindowBackdropType backdropType)
    {
        var pvAttr = 1u;
        var dwmAttr = DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT;

        if (Environment.OSVersion.Version.Build < 22053)
        {
            if (backdropType != WindowBackdropType.None)
                ApplyWindowAttribute(dwmAttr, pvAttr);

            return;
        }

        dwmAttr = DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE;
        pvAttr = (uint)(backdropType switch
        {
            WindowBackdropType.Auto => DWMSBT.AUTO,
            WindowBackdropType.Mica => DWMSBT.MAINWINDOW,
            WindowBackdropType.Acrylic => DWMSBT.TRANSIENTWINDOW,
            WindowBackdropType.Tabbed => DWMSBT.TABBEDWINDOW,
            _ => DWMSBT.DISABLE,        
        });

        ApplyWindowAttribute(dwmAttr, pvAttr);
    }

    protected void ApplyDarkMode()
    {
        var pvAttr = 1u; // 1 to enable, 0 to disable
        var dwmAttr = Environment.OSVersion.Version.Build >= 22523 
            ? DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE 
            : DWMWINDOWATTRIBUTE.DMWA_USE_IMMERSIVE_DARK_MODE_OLD;

        ApplyWindowAttribute(dwmAttr, pvAttr);
    }

    protected void RemoveBackdrop()
    {
        if (_handleSource?.CompositionTarget != null)
        {
            _handleSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
        }

        if (_handleSource?.RootVisual is Window window)
        {
            // TODO move to theme
            window.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
        }
    }

    protected void RemoveTitlebar()
    {
        // to restore focus frame remove it and WS.CAPTION style
        WindowChrome.SetWindowChrome(this, new WindowChrome
        {
            CaptionHeight = 0,
            CornerRadius = default,
            GlassFrameThickness = new Thickness(-1),
            ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
            UseAeroCaptionButtons = false,
        });

        NativeMethods.RemoveTitlebar(_handle);
    }

    protected void RemoveTitlebarBackground()
    {
        if (_handleSource?.Handle == nint.Zero || _handleSource?.CompositionTarget == null)
            return;

        // NOTE: https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute
        // Specifying DWMWA_COLOR_DEFAULT (value 0xFFFFFFFF) for the color will reset the window back to using the system's default behavior for the caption color.
        uint titlebarPvAttribute = 0xFFFFFFFE;

        ApplyWindowAttribute(DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, titlebarPvAttribute);
    }

    private void ApplyWindowAttribute(DWMWINDOWATTRIBUTE dwmAttr, uint pvAttr)
    {
        NativeMethods.DwmSetWindowAttribute(_handle, dwmAttr, ref pvAttr, sizeof(uint));
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
    public DialogWindow()
    {
        MouseDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        };
    }

    public Result<TOut> ShowDialog(TIn data)
    {
        var vm = (IDialogViewModel<TIn, TOut>)DataContext;
        Result<TOut>? result = null;

        vm.OnShow(data, r =>
        {
            result = r;
            Close();
        });

        ShowDialog();
        vm.OnClose();

        return result ?? new Result<TOut>(false);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        RemoveTitlebar();
        RemoveBackdrop();
    }
}
