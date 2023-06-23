using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTool.Enum
{
    [Flags]
    public enum KeyModifier
    {
        None = 0,
        Ctrl = 1,
        Alt = 2,
        Shift = 4,
    }
}
