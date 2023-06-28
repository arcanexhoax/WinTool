using System;
using System.Text;

namespace WinTool.Model
{
    public class CommandLineParameters
    {
        public const string BackgroundParameter = "/background";
        public const string RunAsAdminParameter = "/runAsAdmin";
        public const string CreateFileParameter = "/createFile"; // =filePath

        public bool Background { get; set; }
        public bool RunAsAdmin { get; set; }
        public string? CreateFile { get; set; }

        public static CommandLineParameters Parse(string[] args)
        {
            CommandLineParameters clp = new();

            foreach (var a in args)
            {
                var splitedArg = a.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (splitedArg.Length == 0)
                    continue;

                if (splitedArg.Length == 1)
                {
                    if (splitedArg[0] == BackgroundParameter)
                        clp.Background = true;
                    else if (splitedArg[0] == RunAsAdminParameter)
                        clp.RunAsAdmin = true;
                }
                else
                {
                    if (splitedArg[0] == CreateFileParameter)
                        clp.CreateFile = splitedArg[1];    
                }
            }

            return clp;
        }

        public override string ToString()
        {
            StringBuilder args = new();

            if (Background) args.Append(BackgroundParameter + " ");
            if (RunAsAdmin) args.Append(RunAsAdminParameter + " ");
            if (!string.IsNullOrEmpty(CreateFile)) args.Append($"{CreateFileParameter}={CreateFile}");

            return args.ToString();
        }
    }
}
