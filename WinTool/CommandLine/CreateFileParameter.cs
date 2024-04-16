using System.Text;

namespace WinTool.CommandLine
{
    public class CreateFileParameter : ICommandLineParameter
    {
        public const string ParameterName = "/createFile";
        private const string PathSubParameter = "-path";
        private const string SizeSubParameter = "-size";

        public string? FilePath { get; set; }
        public long Size { get; set; }

        public void Parse(string arg)
        {
            var values = arg.Split('=', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if (values.Length != 2)
                return;

            switch (values[0])
            {
                case PathSubParameter:
                    FilePath = values[1]; 
                    break;
                case SizeSubParameter:
                    if (long.TryParse(values[1], out long size))
                        Size = size;
                    break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new(ParameterName);

            if (FilePath is not (null or []))
                sb.Append($" {PathSubParameter}={FilePath}");
            if (Size > 0)
                sb.Append($" {SizeSubParameter}={Size}");

            return sb.ToString();
        }
    }
}
