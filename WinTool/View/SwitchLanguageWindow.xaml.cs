using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using WinTool.Native;
using WinTool.Utils;
using WinTool.ViewModel;
using Timer = System.Timers.Timer;

namespace WinTool.View
{
    public partial class SwitchLanguageWindow : Window
    {
        private Timer _hideTimer = new(1500) { AutoReset = false };

        public SwitchLanguageWindow(SwitchLanguageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            _hideTimer.Elapsed += (s, e) => Dispatcher.Invoke(HidePopup);

            vm.ShowPopup += caretPos =>
            {
                var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretPos.X, caretPos.Y);
                Left = caretPos.X * DpiUtils.DefaultDpiX / dpiAtPoint;
                Top = caretPos.Y * DpiUtils.DefaultDpiY / dpiAtPoint;

                ShiftWindowToScreen();
                ShowPopup();

                _hideTimer.Stop();
                _hideTimer.Start();
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.ShowWindowAsPopup(hwnd);
        }

        private void ShowPopup()
        {
            if (Visibility == Visibility.Visible)
                return;

            Show();

            var growAnimation = new DoubleAnimation
            {
                From = 0,
                To = 30,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            BeginAnimation(HeightProperty, growAnimation);
        }

        private void HidePopup()
        {
            if (Visibility == Visibility.Hidden)
                return;

            var shrinkAnimation = new DoubleAnimation
            {
                From = ActualHeight,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            shrinkAnimation.Completed += (s, e) => Hide();

            BeginAnimation(HeightProperty, shrinkAnimation);
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
}
