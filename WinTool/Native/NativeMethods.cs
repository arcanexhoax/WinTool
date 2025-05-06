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
            var firstInstanceHwnd = FindWindow(null, windowTitle);

            if (firstInstanceHwnd == nint.Zero)
                return false;

            ShowWindow(firstInstanceHwnd, SW_NORMAL);
            SetForegroundWindow(firstInstanceHwnd);

            return true;
        }
    }
}
