using System;

namespace WinTool.Native.Shell;

[Flags]
public enum SVSI : uint
{
    DESELECT = 0x0000,
    SELECT = 0x0001,
    EDIT = 0x0003,
    DESELECTOTHERS = 0x0004,
    ENSUREVISIBLE = 0x0008,
    FOCUSED = 0x0010
}
