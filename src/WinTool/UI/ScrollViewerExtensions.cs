using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WinTool.UI;

public static class ScrollViewerExtensions
{
    public static readonly DependencyProperty AutoHideScrollBarsProperty =
        DependencyProperty.RegisterAttached("AutoHideScrollBars", typeof(bool), typeof(ScrollViewerExtensions), new PropertyMetadata(false, OnAutoHideScrollBarsPropertyChanged));

    private static readonly DependencyProperty AutoHideScrollBarsControllerProperty =
        DependencyProperty.RegisterAttached("AutoHideScrollBarsController", typeof(AutoHideScrollBarsController), typeof(ScrollViewerExtensions));

    public static bool GetAutoHideScrollBars(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoHideScrollBarsProperty);
    }

    public static void SetAutoHideScrollBars(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoHideScrollBarsProperty, value);
    }

    private static void OnAutoHideScrollBarsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            var controller = new AutoHideScrollBarsController(scrollViewer);
            scrollViewer.SetValue(AutoHideScrollBarsControllerProperty, controller);
            controller.Attach();
        }
        else if (scrollViewer.GetValue(AutoHideScrollBarsControllerProperty) is AutoHideScrollBarsController controller)
        {
            controller.Detach();
            scrollViewer.ClearValue(AutoHideScrollBarsControllerProperty);
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
    {
        var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(dependencyObject, i);

            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class AutoHideScrollBarsController
    {
        private static readonly TimeSpan HideDelay = TimeSpan.FromMilliseconds(900);
        private static readonly Duration ShowAnimationDuration = new(TimeSpan.FromMilliseconds(120));
        private static readonly Duration HideAnimationDuration = new(TimeSpan.FromMilliseconds(180));

        private readonly ScrollViewer _scrollViewer;
        private readonly DispatcherTimer _hideTimer;
        private readonly List<ScrollBar> _scrollBars = [];
        private readonly Dictionary<ScrollBar, bool> _scrollBarVisibilityStates = [];

        public AutoHideScrollBarsController(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _hideTimer = new DispatcherTimer(DispatcherPriority.Normal, _scrollViewer.Dispatcher)
            {
                Interval = HideDelay,
            };
            _hideTimer.Tick += OnHideTimerTick;
        }

        public void Attach()
        {
            _scrollViewer.Loaded += OnLoaded;
            _scrollViewer.Unloaded += OnUnloaded;
            _scrollViewer.MouseEnter += OnMouseStateChanged;
            _scrollViewer.MouseLeave += OnMouseStateChanged;
            _scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            _scrollViewer.ScrollChanged += OnScrollChanged;

            if (_scrollViewer.IsLoaded)
            {
                Initialize();
            }
        }

        public void Detach()
        {
            _hideTimer.Stop();
            _hideTimer.Tick -= OnHideTimerTick;

            _scrollViewer.Loaded -= OnLoaded;
            _scrollViewer.Unloaded -= OnUnloaded;
            _scrollViewer.MouseEnter -= OnMouseStateChanged;
            _scrollViewer.MouseLeave -= OnMouseStateChanged;
            _scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
            _scrollViewer.ScrollChanged -= OnScrollChanged;

            _scrollBars.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _hideTimer.Stop();
            _scrollBars.Clear();
            _scrollBarVisibilityStates.Clear();
        }

        private void OnMouseStateChanged(object sender, MouseEventArgs e)
        {
            UpdateScrollBarsVisibility();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ShowScrollBarsTemporarily();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange == 0 && e.VerticalChange == 0 && e.ExtentWidthChange == 0 && e.ExtentHeightChange == 0)
            {
                UpdateScrollBarsVisibility();
                return;
            }

            ShowScrollBarsTemporarily();
        }

        private void OnHideTimerTick(object? sender, EventArgs e)
        {
            _hideTimer.Stop();
            UpdateScrollBarsVisibility();
        }

        private void Initialize()
        {
            RefreshScrollBars();
            UpdateScrollBarsVisibility();
        }

        private void RefreshScrollBars()
        {
            _scrollViewer.ApplyTemplate();

            _scrollBars.Clear();
            _scrollBarVisibilityStates.Clear();
            _scrollBars.AddRange(FindVisualChildren<ScrollBar>(_scrollViewer));
        }

        private void ShowScrollBarsTemporarily()
        {
            if (_scrollBars.Count == 0)
            {
                RefreshScrollBars();
            }

            SetScrollBarsVisibility(isVisible: true);

            if (_scrollViewer.IsMouseOver)
            {
                _hideTimer.Stop();
                return;
            }

            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void UpdateScrollBarsVisibility()
        {
            if (_scrollBars.Count == 0)
            {
                RefreshScrollBars();
            }

            SetScrollBarsVisibility(_scrollViewer.IsMouseOver || _hideTimer.IsEnabled);
        }

        private void SetScrollBarsVisibility(bool isVisible)
        {
            foreach (var scrollBar in _scrollBars)
            {
                if (scrollBar.Maximum <= scrollBar.Minimum)
                {
                    SetScrollBarVisibility(scrollBar, isVisible: false);
                    continue;
                }

                SetScrollBarVisibility(scrollBar, isVisible);
            }
        }

        private void SetScrollBarVisibility(ScrollBar scrollBar, bool isVisible)
        {
            if (_scrollBarVisibilityStates.TryGetValue(scrollBar, out var currentVisibility) && currentVisibility == isVisible)
            {
                return;
            }

            _scrollBarVisibilityStates[scrollBar] = isVisible;

            if (isVisible)
            {
                scrollBar.IsHitTestVisible = true;
                scrollBar.BeginAnimation(UIElement.OpacityProperty, CreateOpacityAnimation(1, ShowAnimationDuration));
                return;
            }

            var animation = CreateOpacityAnimation(0, HideAnimationDuration);
            animation.Completed += (_, _) =>
            {
                if (_scrollBarVisibilityStates.TryGetValue(scrollBar, out var latestVisibility) && !latestVisibility)
                {
                    scrollBar.IsHitTestVisible = false;
                    scrollBar.Opacity = 0;
                }
            };

            scrollBar.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private static DoubleAnimation CreateOpacityAnimation(double to, Duration duration)
        {
            return new DoubleAnimation
            {
                To = to,
                Duration = duration,
                EasingFunction = new QuadraticEase(),
            };
        }
    }
}