namespace WinTool.CommandLine
{
    public class BackgroundParameter : ICommandLineParameter
    {
        public const string ParameterName = "/background";

        public void Parse(string arg) { }

        public override string ToString() => ParameterName;
    }
}
