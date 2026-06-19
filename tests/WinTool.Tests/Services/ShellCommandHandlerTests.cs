using WinTool.Services;

namespace WinTool.Tests.Services;

public class ShellCommandHandlerTests : IDisposable
{
    private const string FileName = "New Text Document.txt";

    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "WinTool.Tests", Guid.NewGuid().ToString("N"));

    public ShellCommandHandlerTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void GetAvailableFilePath_WhenNameIsAvailable_ReturnsOriginalName()
    {
        string path = ShellCommandHandler.GetAvailableFilePath(_tempDirectory, FileName);

        Assert.Equal(Path.Combine(_tempDirectory, FileName), path);
    }

    [Fact]
    public void GetAvailableFilePath_WhenNameExists_AppendsNumberBeforeExtension()
    {
        File.Create(Path.Combine(_tempDirectory, FileName)).Dispose();

        string path = ShellCommandHandler.GetAvailableFilePath(_tempDirectory, FileName);

        Assert.Equal(Path.Combine(_tempDirectory, "New Text Document (2).txt"), path);
    }

    [Fact]
    public void GetAvailableFilePath_WhenNumberIsAvailable_ReusesFirstGap()
    {
        File.Create(Path.Combine(_tempDirectory, FileName)).Dispose();
        File.Create(Path.Combine(_tempDirectory, "New Text Document (3).txt")).Dispose();

        string path = ShellCommandHandler.GetAvailableFilePath(_tempDirectory, FileName);

        Assert.Equal(Path.Combine(_tempDirectory, "New Text Document (2).txt"), path);
    }

    [Fact]
    public void GetAvailableFilePath_WhenSecondNameExists_ReturnsThirdName()
    {
        File.Create(Path.Combine(_tempDirectory, FileName)).Dispose();
        File.Create(Path.Combine(_tempDirectory, "New Text Document (2).txt")).Dispose();

        string path = ShellCommandHandler.GetAvailableFilePath(_tempDirectory, FileName);

        Assert.Equal(Path.Combine(_tempDirectory, "New Text Document (3).txt"), path);
    }

    [Fact]
    public void GetNewFileName_ReturnsShellLocalizedNameWithTextExtension()
    {
        string fileName = ShellCommandHandler.GetNewFileName();

        Assert.EndsWith(".txt", fileName, StringComparison.OrdinalIgnoreCase);
        Assert.True(fileName.Length > ".txt".Length);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDirectory, true);
    }
}
