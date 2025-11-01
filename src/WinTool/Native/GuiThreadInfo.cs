using System;
using System.Runtime.InteropServices;

namespace WinTool.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public GuiThreadInfoFlags flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public System.Drawing.Rectangle rcCaret;
    }

    [Flags]
    public enum GuiThreadInfoFlags
    {
        GUI_CARETBLINKING = 0x00000001,
        GUI_INMENUMODE = 0x00000004,
        GUI_INMOVESIZE = 0x00000002,
        GUI_POPUPMENUMODE = 0x00000010,
        GUI_SYSTEMMENUMODE = 0x00000008
    }
}
