using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using WinTool.Native;
using WinTool.Utils;
using WinTool.ViewModels.Features;
using Timer = System.Timers.Timer;

namespace WinTool.Views.Features;

public partial class InputPopupWindow : FluentWindow
{
    private const double AnimTimeMs = 200;
    private const double OffsetY = 20;

    private readonly IKeyInterceptor _keyInterceptor;
    private readonly Timer _hideTimer = new(1500) { AutoReset = false };

    private bool _isHiding;
    private bool _flippedAbove;
    private double _animatedTop;
    private (double X, double Y) _lastPosition;
    private Guid _currentHideAnimGuid;

    public InputPopupWindow(InputPopupViewModel vm, IKeyInterceptor keyInterceptor)
    {
        InitializeComponent();
        DataContext = vm;

        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
        _hideTimer.Elapsed += (s, e) => Dispatcher.Invoke(HidePopup);

        vm.ShowPopup += caretPos =>
        {
            Dispatcher.Invoke(() =>
            {
                _hideTimer.Stop();

                var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretPos.X, caretPos.Y);
                var x = caretPos.X * DpiUtils.DefaultDpiX / dpiAtPoint;
                var y = caretPos.Y * DpiUtils.DefaultDpiY / dpiAtPoint;

                ShowPopup(x, y);

                _hideTimer.Start();
            });
        };
    }
    
    private void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        if (e.Shortcut.Modifier is KeyModifier.Shift or KeyModifier.None
            && !e.Shortcut.Key.IsModifier 
            && e.Shortcut.State is KeyState.Down)
        {
            HidePopup();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        NativeMethods.ShowWindowAsPopup(_handle);
        RemoveTitlebar();
        ApplyBackdrop(WindowBackdropType.Acrylic);

        _handleSource!.AddHook(WndProc);
    }

    private void ShowPopup(double x, double y)
    {
        if (Visibility == Visibility.Visible && _currentHideAnimGuid == Guid.Empty && (x, y) == _lastPosition)
            return;

        var (finalTop, flipped) = AdjustToScreen(x, y, Height);
        _flippedAbove = flipped;
        _animatedTop = finalTop;

        var animFrom = flipped ? finalTop + OffsetY : finalTop - OffsetY;
        Top = animFrom;

        _lastPosition = (x, y);
        _isHiding = false;
        _currentHideAnimGuid = Guid.Empty;

        Show();
        NativeMethods.DefWindowProc(_handle, NativeMethods.WM_NCACTIVATE, 1, 0);

        var slide = new DoubleAnimation
        {
            From = animFrom,
            To = finalTop,
            Duration = TimeSpan.FromMilliseconds(AnimTimeMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(TopProperty, slide);
    }

    private void HidePopup()
    {
        if (Visibility == Visibility.Hidden || _isHiding)
            return;

        _isHiding = true;

        var animGuid = _currentHideAnimGuid = Guid.NewGuid();
        var hideTo = _flippedAbove ? _animatedTop + OffsetY : _animatedTop - OffsetY;

        var slide = new DoubleAnimation
        {
            From = _animatedTop,
            To = hideTo,
            Duration = TimeSpan.FromMilliseconds(AnimTimeMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        slide.Completed += (s, e) =>
        {
            if (animGuid == _currentHideAnimGuid)
            {
                _currentHideAnimGuid = Guid.Empty;
                _isHiding = false;

                Hide();
                BeginAnimation(TopProperty, null);
            }
        };

        BeginAnimation(TopProperty, slide);
    }

    private (double FinalTop, bool FlippedAbove) AdjustToScreen(double caretX, double caretY, double height)
    {
        var width = ActualWidth;

        if (width == 0 && Content is UIElement content)
        {
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            width = content.DesiredSize.Width;
        }

        var windowPoint = new System.Drawing.Point((int)caretX, (int)caretY);
        var activeScreen = Screen.FromPoint(windowPoint);
        var screenRight = activeScreen.WorkingArea.X + activeScreen.WorkingArea.Width;
        var screenBottom = activeScreen.WorkingArea.Y + activeScreen.WorkingArea.Height;

        Left = caretX;

        if (Left + width > screenRight)
            Left = screenRight - width;

        var flipped = caretY + height > screenBottom;
        var finalTop = flipped ? caretY - height : caretY;

        return (finalTop, flipped);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_NCACTIVATE)
        {
            handled = true;
            return NativeMethods.DefWindowProc(hwnd, (uint)msg, 1, lParam);
        }

        return nint.Zero;
    }
}
