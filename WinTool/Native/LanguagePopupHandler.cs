using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using UIAutomationClient;
using WinTool.Utils;
using WinTool.View;

namespace WinTool.Native
{
    public class LanguagePopupHandler(SwitchLanguageWindow changeLanguageWindow)
    {
        public void Show()
        {
            var caretRect = GetCaretRect(changeLanguageWindow.Width, changeLanguageWindow.Height);

            // https://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
            var dpiAtPoint = DpiUtils.GetDpiForNearestMonitor(caretRect.right, caretRect.bottom);
            changeLanguageWindow.Left = caretRect.right * DpiUtils.DefaultDpiX / dpiAtPoint;
            changeLanguageWindow.Top = caretRect.bottom * DpiUtils.DefaultDpiY / dpiAtPoint;

            ShiftWindowToScreen(changeLanguageWindow);
            changeLanguageWindow.Show();
        }

        public RECT GetCaretRect(double width, double height)
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

        private static RECT GetAccessibleCaretRect(IntPtr hwnd)
        {
            var guid = typeof(IAccessible).GUID;
            object? accessibleObject = null;
            var retVal = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.OBJID_CARET, ref guid, ref accessibleObject);

            if (retVal != 0 || accessibleObject is not IAccessible accessible)
                throw new Exception($"Error getting caret rectangle: {retVal}");

            accessible.accLocation(out int left, out int top, out int width, out int height, NativeMethods.CHILDID_SELF);
            return new RECT() { bottom = top + height, left = left, right = left + width, top = top };
        }

        private static RECT GetWinApiCaretRect(IntPtr hwnd)
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
