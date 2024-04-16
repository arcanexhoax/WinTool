namespace WinTool.CommandLine
{
    public class ShutdownOnEndedParameter : ICommandLineParameter
    {
        public const string ParameterName = "/shutdownOnEnded";

        public void Parse(string arg) { }

        public override string ToString() => ParameterName;
    }
}
