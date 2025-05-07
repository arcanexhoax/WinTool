using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using WinTool.Native;
using WinTool.Utils;
using WinTool.ViewModel;

namespace WinTool.View
{
    public partial class SwitchLanguageWindow : Window
    {
        public SwitchLanguageWindow(SwitchLanguageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.ShowPopup += caretPos =>
            {
                // https://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
                var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretPos.X, caretPos.Y);
                Left = caretPos.X * DpiUtils.DefaultDpiX / dpiAtPoint;
                Top = caretPos.Y * DpiUtils.DefaultDpiY / dpiAtPoint;

                ShiftWindowToScreen();
                Show();
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.ShowWindowAsPopup(hwnd);
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
