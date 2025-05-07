using System;
using System.Runtime.InteropServices;
using System.Text;
using WinTool.Utils;

namespace WinTool.Native
{
    public class NativeMethods
    {
        private const int SW_NORMAL = 1;
        public const int CHILDID_SELF = 0;
        public const uint OBJID_CARET = 0xFFFFFFF8;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private static readonly IntPtr HWND_TOPMOST = new(-1);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32")]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport("user32")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("d2d1")]
        internal static extern int D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr pFactoryOptions, out ID2D1Factory ppIFactory);

        [DllImport("user32.dll")]
        internal static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromWindow(
         IntPtr hwnd,
         uint id,
         ref Guid iid,
         [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? ppvObject);


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetCaretPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        internal static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[]? lpList);

        public static string? GetTextFrom(nint hWnd, Func<nint, StringBuilder, int, int> getText)
        {
            string? text = null;
            var buff = new StringBuilder(256);

            if (getText(hWnd, buff, buff.Capacity) > 0)
                text = buff.ToString();

            return text;
        }

        public static string? GetWindowText(nint hWnd) => GetTextFrom(hWnd, GetWindowText);

        public static string? GetClassName(nint hWnd) => GetTextFrom(hWnd, GetClassName);

        public static bool ShowWindow(string windowTitle)
        {
            var hwnd = FindWindow(null, windowTitle);

            if (hwnd == nint.Zero)
                return false;

            ShowWindow(hwnd, SW_NORMAL);
            SetForegroundWindow(hwnd);

            return true;
        }

        public static void ShowWindowAsPopup(nint hwnd)
        {
            int exStyle = (int)GetWindowLong(hwnd, GWL_EXSTYLE);

            exStyle |= WS_EX_TOOLWINDOW;
            exStyle |= WS_EX_NOACTIVATE;

            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }
}
