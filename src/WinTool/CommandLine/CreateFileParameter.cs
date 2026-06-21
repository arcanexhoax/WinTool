using System.Text;

namespace WinTool.CommandLine;

public class CreateFileParameter : ICommandLineParameter
{
    public const string ParameterName = "/createFile";
    private const string PathSubParameter = "-path";

    public string? FilePath { get; set; }

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
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(ParameterName);

        if (FilePath is not (null or []))
            sb.Append($" {PathSubParameter}=\"{FilePath}\"");

        return sb.ToString();
    }
}
