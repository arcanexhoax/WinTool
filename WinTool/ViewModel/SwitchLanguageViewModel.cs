using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using WinTool.Native;
using UIAutomationClient;
using System.Drawing;
using WinTool.Services;
using System.Threading.Tasks;

namespace WinTool.ViewModel
{
    public class SwitchLanguageViewModel : ObservableObject
    {
        private readonly KeyboardLayoutManager _keyboardLayoutManager;

        public string? CurrentLanguage
        {
            get; set => SetProperty(ref field, value);
        }

        public event Action<Point>? ShowPopup;

        public SwitchLanguageViewModel(KeyboardLayoutManager keyboardLayoutManager)
        {
            _keyboardLayoutManager = keyboardLayoutManager;
            _keyboardLayoutManager.LayoutChanged += OnLayoutChanged;
        }

        public async Task Init()
        {
            await _keyboardLayoutManager.Start();
        }

        public void OnLayoutChanged(CultureInfo newLayout)
        {
            CurrentLanguage = newLayout.ThreeLetterISOLanguageName.ToUpper();
            var caretRect = GetCaretRect();

            if (caretRect == null)
                return;

            ShowPopup?.Invoke(new Point(caretRect.Value.right, caretRect.Value.bottom));
        }

        private RECT? GetCaretRect()
        {
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(info);

            if (!NativeMethods.GetGUIThreadInfo(0, ref info))
                return null;

            var hwndFocus = info.hwndFocus;
            var caretRect = GetAccessibleCaretRect(hwndFocus);

            if (RectValid(caretRect))
                return caretRect;

            caretRect = GetWinApiCaretRect(hwndFocus);

            if (RectValid(caretRect))
                return caretRect;

            return null;
        }

        private RECT? GetAccessibleCaretRect(nint hwnd)
        {
            var guid = typeof(IAccessible).GUID;
            object? accessibleObject = null;
            var retVal = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.OBJID_CARET, ref guid, ref accessibleObject);

            if (retVal != 0 || accessibleObject is not IAccessible accessible)
                return null;

            accessible.accLocation(out int left, out int top, out int width, out int height, NativeMethods.CHILDID_SELF);

            return new RECT() 
            { 
                bottom = top + height, 
                left = left, 
                right = left + width, 
                top = top 
            };
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

        private static bool RectValid(RECT? rect)
        {
            return rect is { bottom: > 0, left: > 0, right: > 0, top: > 0 };
        }
    }
}
