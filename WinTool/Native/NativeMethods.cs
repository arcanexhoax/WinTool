using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTool.Native
{
    public class NativeMethods
    {
        private const int SW_NORMAL = 1;

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
