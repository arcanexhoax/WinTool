using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTool.Native
{
    public class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string? GetWindowText(IntPtr hWnd)
        {
            string? text = null;
            const int nChars = 256;
            StringBuilder buff = new(nChars);

            if (GetWindowText(hWnd, buff, nChars) > 0)
                text = buff.ToString();

            return text;
        }
    }
}
