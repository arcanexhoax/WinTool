using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTool.CommandLine
{
    internal interface ICommandLineParameter
    {
        void Parse(string arg);
    }
}
