using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinTool.Native;
using WinTool.Utils;
using WinTool.ViewModels.Features;
using Timer = System.Timers.Timer;

namespace WinTool.Views.Features;

public partial class InputPopupWindow : FluentWindow
{
    private const double AppearAnimTimeMs = 200;
    private const double AppearOffsetY = 20;
    private const double SelectionAnimTimeMs = 150;
    public const double SelectionItemWidth = 48;

    private readonly IKeyInterceptor _keyInterceptor;
    private readonly InputPopupViewModel _viewModel;
    private readonly Timer _hideTimer = new(1500) { AutoReset = false };

    private int _lastIndicatorIndex = -1;
    private bool _isHiding;
    private bool _flippedAbove;
    private double _animatedTop;
    private (double X, double Y) _lastPosition;
    private Guid _currentHideAnimGuid;

    public InputPopupWindow(InputPopupViewModel vm, IKeyInterceptor keyInterceptor)
    {
        InitializeComponent();
        DataContext = vm;

        _viewModel = vm;
        _keyInterceptor = keyInterceptor;
        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
        _hideTimer.Elapsed += (s, e) => Dispatcher.Invoke(HidePopup);

        vm.ShowPopup += caretPos =>
        {
            Dispatcher.Invoke(() =>
            {
                _hideTimer.Stop();

                UpdateSelectionIndicatorPosition();

                var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretPos.X, caretPos.Y);
                var x = caretPos.X * DpiUtils.DefaultDpiX / dpiAtPoint;
                var y = caretPos.Y * DpiUtils.DefaultDpiY / dpiAtPoint;

                ShowPopup(x, y);

                UpdateSelectionIndicatorPosition();

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

        var (finalTop, flipped) = AdjustWindowToScreen(x, y, Height);
        _flippedAbove = flipped;
        _animatedTop = finalTop;

        var animFrom = flipped ? finalTop + AppearOffsetY : finalTop - AppearOffsetY;
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
            Duration = TimeSpan.FromMilliseconds(AppearAnimTimeMs),
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
        var hideTo = _flippedAbove ? _animatedTop + AppearOffsetY : _animatedTop - AppearOffsetY;

        var slide = new DoubleAnimation
        {
            From = _animatedTop,
            To = hideTo,
            Duration = TimeSpan.FromMilliseconds(AppearAnimTimeMs),
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

    private void UpdateSelectionIndicatorPosition()
    {
        if (_viewModel.AllLanguages is null or [])
            return;

        int selectedIndex = -1;

        foreach (var (i, lang) in _viewModel.AllLanguages.Index())
        {
            if (lang.IsSelected)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex < 0)
            return;

        if (_lastIndicatorIndex >= _viewModel.AllLanguages.Count)
            _lastIndicatorIndex = -1;

        var (targetX, itemWidth) = GetSelectionItemMetrics(selectedIndex);

        if (itemWidth > 0)
            SelectionOverlay.Width = itemWidth;

        if (_lastIndicatorIndex < 0 || Visibility != Visibility.Visible || _isHiding)
        {
            SelectionTranslate.BeginAnimation(TranslateTransform.XProperty, null);
            AccentScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);

            SelectionTranslate.X = targetX;
            AccentScale.ScaleX = 1.0;

            _lastIndicatorIndex = selectedIndex;
            return;
        }

        if (selectedIndex == _lastIndicatorIndex)
            return;

        _lastIndicatorIndex = selectedIndex;
        AnimateSelectionIndicator(targetX);
    }

    private (double X, double Width) GetSelectionItemMetrics(int index)
    {
        if (LanguageItemsControl.ItemContainerGenerator.ContainerFromIndex(index) is FrameworkElement container)
        {
            var pos = container.TransformToAncestor(LanguageItemsControl).Transform(new Point(0, 0));
            return (pos.X, container.ActualWidth);
        }

        return (index * SelectionItemWidth, SelectionItemWidth);
    }

    private void AnimateSelectionIndicator(double targetX)
    {
        var duration = TimeSpan.FromMilliseconds(SelectionAnimTimeMs);

        var slideAnim = new DoubleAnimationUsingKeyFrames { Duration = duration };
        slideAnim.KeyFrames.Add(new SplineDoubleKeyFrame(targetX, KeyTime.FromPercent(1.0), new KeySpline(0.8, 0, 0.2, 1)));

        var squeezeAnim = new DoubleAnimationUsingKeyFrames { Duration = duration };
        squeezeAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0.6, KeyTime.FromPercent(0.167), new KeySpline(0, 0, 0, 1)));
        squeezeAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.6, KeyTime.FromPercent(0.667)));
        squeezeAnim.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0), new KeySpline(0, 0, 0, 1)));

        SelectionTranslate.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        AccentScale.BeginAnimation(ScaleTransform.ScaleXProperty, squeezeAnim);
    }

    private (double FinalTop, bool FlippedAbove) AdjustWindowToScreen(double caretX, double caretY, double height)
    {
        var width = ActualWidth;

        if (width == 0 && Content is UIElement content)
        {
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            width = content.DesiredSize.Width;
        }

        var workingArea = DpiUtils.GetWorkingAreaAt((int)caretX, (int)caretY);
        var screenRight = workingArea.Right;
        var screenBottom = workingArea.Bottom;

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
