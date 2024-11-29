using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTool.Native
{
    public class NativeMethods
    {
        private const int SW_NORMAL = 1;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
