using System;
using System.Runtime.InteropServices;
using System.Text;
using static WinTool.View.CreateFileView;

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

        public static string? GetWindowText(IntPtr hWnd)
        {
            string? text = null;
            const int nChars = 256;
            StringBuilder buff = new(nChars);

            if (GetWindowText(hWnd, buff, nChars) > 0)
                text = buff.ToString();

            return text;
        }

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
