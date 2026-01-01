using WinTool.CommandLine;

namespace WinTool.Tests.CommandLine;

public class CommandLineParametersTests
{
    [Fact]
    public void Parse_BackgroundParameter_Works()
    {
        var args = new[] { "/background" };
        var result = CommandLineParameters.Parse(args);

        Assert.NotNull(result.BackgroundParameter);
        Assert.IsType<BackgroundParameter>(result.BackgroundParameter);
        Assert.Null(result.CreateFileParameter);
        Assert.Null(result.ShutdownOnEndedParameter);
    }

    [Fact]
    public void Parse_ShutdownOnEndedParameter_Works()
    {
        var args = new[] { "/shutdownOnEnded" };
        var result = CommandLineParameters.Parse(args);

        Assert.NotNull(result.ShutdownOnEndedParameter);
        Assert.IsType<ShutdownOnEndedParameter>(result.ShutdownOnEndedParameter);
        Assert.Null(result.BackgroundParameter);
        Assert.Null(result.CreateFileParameter);
    }

    [Fact]
    public void Parse_CreateFileParameter_Works()
    {
        var args = new[] { "/createFile", "-path=test.txt", "-size=12345" };
        var result = CommandLineParameters.Parse(args);

        Assert.NotNull(result.CreateFileParameter);
        Assert.IsType<CreateFileParameter>(result.CreateFileParameter);
        Assert.Equal("test.txt", result.CreateFileParameter!.FilePath);
        Assert.Equal(12345, result.CreateFileParameter.Size);
        Assert.Null(result.BackgroundParameter);
        Assert.Null(result.ShutdownOnEndedParameter);
    }

    [Fact]
    public void Parse_Invalid_CreateFileParameter()
    {
        var args = new[] { "/createFile", "-path=", "-size=abc" };
        var result = CommandLineParameters.Parse(args);

        Assert.NotNull(result.CreateFileParameter);
        Assert.IsType<CreateFileParameter>(result.CreateFileParameter);
        Assert.Null(result.CreateFileParameter!.FilePath);
        Assert.Equal(0, result.CreateFileParameter.Size);
        Assert.Null(result.BackgroundParameter);
        Assert.Null(result.ShutdownOnEndedParameter);
    }

    [Fact]
    public void Parse_Invalid_Parameters()
    {
        var args = new[] { "/invalidParameter", "/invalid_parameter", "--invalid-parameter", "invalidParameter", "invalid_parameter", "invalid-parameter" };
        var result = CommandLineParameters.Parse(args);

        Assert.NotNull(result);
        Assert.Null(result.CreateFileParameter);
        Assert.Null(result.BackgroundParameter);
        Assert.Null(result.ShutdownOnEndedParameter);
    }

    [Fact]
    public void ToString_Of_Parameters_Produces_Correct_CommandLine()
    {
        var background = new BackgroundParameter();
        var shutdown = new ShutdownOnEndedParameter();
        var createFile = new CreateFileParameter { FilePath = "file.bin", Size = 1000 };

        Assert.Equal("/background", background.ToString());
        Assert.Equal("/shutdownOnEnded", shutdown.ToString());
        Assert.Equal("/createFile -path=\"file.bin\" -size=1000", createFile.ToString());
    }

    [Fact]
    public void ToString_Of_CommandLineParameters_Produces_Correct_CommandLine()
    {
        var clp = new CommandLineParameters
        {
            BackgroundParameter = new BackgroundParameter(),
            CreateFileParameter = new CreateFileParameter { FilePath = "abc.txt", Size = 42 },
            ShutdownOnEndedParameter = new ShutdownOnEndedParameter()
        };
        var result = clp.ToString().Trim();

        Assert.Contains("/background", result);
        Assert.Contains("/createFile -path=\"abc.txt\" -size=42", result);
        Assert.Contains("/shutdownOnEnded", result);
    }
}
