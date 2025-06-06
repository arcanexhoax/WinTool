﻿using System.Runtime.InteropServices;

namespace WinTool.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
