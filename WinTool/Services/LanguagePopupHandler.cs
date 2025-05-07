using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using UIAutomationClient;
using WinTool.Native;
using WinTool.Utils;
using WinTool.View;

namespace WinTool.Services
{
    public class LanguagePopupHandler
    {
        private readonly SwitchLanguageWindow _changeLanguageWindow;

        private nint _lastLanguage = nint.Zero;

        public LanguagePopupHandler(SwitchLanguageWindow changeLanguageWindow)
        {
            _changeLanguageWindow = changeLanguageWindow;
        }

        public async Task Start()
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            while (await timer.WaitForNextTickAsync())
            {
                var foregroundWindow = NativeMethods.GetForegroundWindow();
                var threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);
                var currentLayout = NativeMethods.GetKeyboardLayout(threadId);

                if (currentLayout != _lastLanguage)
                {
                    _lastLanguage = currentLayout;

                    var culture = CultureInfo.GetCultureInfo((short)currentLayout.ToInt64());
                    var languageName = culture.ThreeLetterISOLanguageName;
                    Debug.WriteLine($"New layout: {languageName}");
                    ShowPopup();
                }
            }
        }

        public void ShowPopup()
        {
            var caretRect = GetCaretRect(_changeLanguageWindow.Width, _changeLanguageWindow.Height);

            // https://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
            var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretRect.right, caretRect.bottom);
            _changeLanguageWindow.Left = caretRect.right * DpiUtils.DefaultDpiX / dpiAtPoint;
            _changeLanguageWindow.Top = caretRect.bottom * DpiUtils.DefaultDpiY / dpiAtPoint;

            ShiftWindowToScreen(_changeLanguageWindow);
            _changeLanguageWindow.Show();
        }

        private RECT GetCaretRect(double width, double height)
        {
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(info);

            if (!NativeMethods.GetGUIThreadInfo(0, ref info))
                return new RECT();

            var hwndFocus = info.hwndFocus;
            var caretRect = GetAccessibleCaretRect(hwndFocus);

            if (RectValid(caretRect))
                return caretRect;

            caretRect = GetWinApiCaretRect(hwndFocus);

            if (RectValid(caretRect))
                return caretRect;

            return new RECT()
            {
                left = (int)(SystemParameters.PrimaryScreenWidth - width),
                top = (int)(SystemParameters.PrimaryScreenHeight - height),
                right = (int)SystemParameters.PrimaryScreenWidth,
                bottom = (int)SystemParameters.PrimaryScreenHeight
            };
        }

        private RECT GetAccessibleCaretRect(nint hwnd)
        {
            var guid = typeof(IAccessible).GUID;
            object? accessibleObject = null;
            var retVal = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.OBJID_CARET, ref guid, ref accessibleObject);

            if (retVal != 0 || accessibleObject is not IAccessible accessible)
                throw new Exception($"Error getting caret rectangle: {retVal}");

            accessible.accLocation(out int left, out int top, out int width, out int height, NativeMethods.CHILDID_SELF);
            return new RECT() { bottom = top + height, left = left, right = left + width, top = top };
        }

        private RECT GetWinApiCaretRect(nint hwnd)
        {
            // Try WinAPI
            uint idAttach = 0;
            uint curThreadId = 0;
            POINT caretPoint;

            try
            {
                idAttach = NativeMethods.GetWindowThreadProcessId(hwnd, out uint id);
                curThreadId = NativeMethods.GetCurrentThreadId();

                // To attach to current thread
                var sa = NativeMethods.AttachThreadInput(idAttach, curThreadId, true);
                var caretPos = NativeMethods.GetCaretPos(out caretPoint);
                NativeMethods.ClientToScreen(hwnd, ref caretPoint);
            }
            finally
            {
                // To dettach from current thread
                var sd = NativeMethods.AttachThreadInput(idAttach, curThreadId, false);
            }

            return new RECT()
            {
                left = caretPoint.X,
                top = caretPoint.Y,
                bottom = caretPoint.Y + 20,
                right = caretPoint.X + 1
            };
        }

        private static bool RectValid(RECT rect)
        {
            return rect is { bottom: > 0, left: > 0, right: > 0, top: > 0 };
        }

        /// <summary>
        /// Shifts window to nearest screen if it's out of it.
        /// </summary>
        /// <param name="window">Target window.</param>
        public static void ShiftWindowToScreen(Window window)
        {
            var windowPoint = new System.Drawing.Point((int)window.Left, (int)window.Top);
            var activeScreen = Screen.FromPoint(windowPoint);
            var windowRight = window.Left + window.Width;
            var screenRight = activeScreen.WorkingArea.X + activeScreen.WorkingArea.Width;

            if (windowRight > screenRight)
            {
                window.Left = screenRight - window.Width;
            }

            var windowBottom = window.Top + window.Height;
            var screenBottom = activeScreen.WorkingArea.Y + activeScreen.WorkingArea.Height;

            if (windowBottom > screenBottom)
            {
                window.Top = screenBottom - window.Height;
            }
        }
    }
}
