using WinTool.Services;

namespace WinTool.Tests.Services;

public class ProcessHelperTests
{
    [Fact]
    public void GetCmdArguments_WithoutArguments_QuotesFilePath()
    {
        string arguments = ProcessHelper.GetCmdArguments(@"C:\Program Files\Tool\tool.exe", null);

        Assert.Equal("/d /s /v:off /k \"\"C:\\Program Files\\Tool\\tool.exe\"\"", arguments);
    }

    [Fact]
    public void GetCmdArguments_WithCmdMetacharacters_EscapesUnquotedArguments()
    {
        string arguments = ProcessHelper.GetCmdArguments(@"C:\Tools\tool.exe", "value&more (test)");

        Assert.Equal("/d /s /v:off /k \"\"C:\\Tools\\tool.exe\" value^&more ^(test^)\"", arguments);
    }

    [Fact]
    public void GetCmdArguments_WithCmdMetacharacters_EscapesFilePath()
    {
        string arguments = ProcessHelper.GetCmdArguments(@"C:\Tools\%USERNAME% & (test)\tool.exe", null);

        Assert.Equal("/d /s /v:off /k \"\"C:\\Tools\\^%USERNAME^% ^& ^(test^)\\tool.exe\"\"", arguments);
    }
}
