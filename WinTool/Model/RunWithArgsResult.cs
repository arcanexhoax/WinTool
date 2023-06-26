using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTool.Model
{
    public class RunWithArgsResult
    {
        public bool Success { get; }
        public string? Args { get; }

        public RunWithArgsResult(bool success, string? args)
        {
            Success = success;
            Args = args;
        }
    }
}
