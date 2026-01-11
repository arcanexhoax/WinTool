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

public partial class SwitchLanguageWindow : FluentWindow
{
    private const double AnimTimeMs = 200;
    private const double OffsetY = 20;

    private readonly IKeyInterceptor _keyInterceptor;
    private readonly Timer _hideTimer = new(1500) { AutoReset = false };

    private bool _isHiding;
    private (double X, double Y) _lastPosition;
    private Guid _currentHideAnimGuid;

    public SwitchLanguageWindow(SwitchLanguageViewModel vm, IKeyInterceptor keyInterceptor)
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
    }

    private void ShowPopup(double x, double y)
    {
        if (Visibility == Visibility.Visible && _currentHideAnimGuid == Guid.Empty && (x, y) == _lastPosition)
            return;

        Left = x;
        Top = y - OffsetY;

        _lastPosition = (x, y);
        _isHiding = false;
        _currentHideAnimGuid = Guid.Empty;

        Show();

        var slide = new DoubleAnimation
        {
            From = y - OffsetY,
            To = y,
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

        var slide = new DoubleAnimation
        {
            From = _lastPosition.Y,
            To = _lastPosition.Y - OffsetY,
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

    public void ShiftWindowToScreen()
    {
        var windowPoint = new System.Drawing.Point((int)Left, (int)Top);
        var activeScreen = Screen.FromPoint(windowPoint);
        var windowRight = Left + Width;
        var screenRight = activeScreen.WorkingArea.X + activeScreen.WorkingArea.Width;

        if (windowRight > screenRight)
        {
            Left = screenRight - Width;
        }

        var windowBottom = Top + Height;
        var screenBottom = activeScreen.WorkingArea.Y + activeScreen.WorkingArea.Height;

        if (windowBottom > screenBottom)
        {
            Top = screenBottom - Height;
        }
    }
}
