using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTool.Enum
{
    public enum KeyState
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }
}
