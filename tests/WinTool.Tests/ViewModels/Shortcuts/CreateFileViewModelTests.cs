using Microsoft.Extensions.Logging;
using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Tests.ViewModels.Shortcuts;

public class CreateFileViewModelTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "WinTool.Tests", Guid.NewGuid().ToString("N"));

    public CreateFileViewModelTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Constructor_UsesDialogStateValues()
    {
        var state = new CreateFileDialogState
        {
            FileName = "draft.txt",
            Size = 2,
            SelectedSizeUnit = SizeUnit.MB
        };

        var viewModel = CreateViewModel(state);

        Assert.Equal("draft.txt", viewModel.FileName);
        Assert.Equal((uint)2, viewModel.Size);
        Assert.Equal(SizeUnit.MB, viewModel.SelectedSizeUnit);
        Assert.Contains(SizeUnit.MB, viewModel.SizeUnits);
    }

    [Fact]
    public void OnShow_SetsFolderStateFromCurrentDialogValues()
    {
        var state = new CreateFileDialogState { Size = 3 };
        var viewModel = CreateViewModel(state);

        viewModel.OnShow(_tempDirectory, _ => { });

        Assert.Equal(new DirectoryInfo(_tempDirectory).Name, viewModel.RelativeFolderPath);
        Assert.Equal(_tempDirectory, viewModel.FullFolderPath);
        Assert.True(viewModel.IsTextSelected);
        Assert.True(viewModel.AreOptionsOpened);
    }

    [Fact]
    public void CreateCommand_ValidInput_ReturnsRequestedFilePathAndSize()
    {
        var state = new CreateFileDialogState
        {
            FileName = "draft.txt",
            Size = 2,
            SelectedSizeUnit = SizeUnit.KB
        };
        var viewModel = CreateViewModel(state);
        Result<CreateFileOutput>? result = null;

        viewModel.OnShow(_tempDirectory, output => result = output);

        viewModel.CreateCommand.Execute(null);

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(Path.Combine(_tempDirectory, "draft.txt"), result.Data!.FilePath);
        Assert.Equal(2 * (long)SizeUnit.KB, result.Data.Size);
    }

    [Fact]
    public void OnClose_PersistsUpdatedState()
    {
        var state = new CreateFileDialogState();
        var viewModel = CreateViewModel(state);
        viewModel.FileName = "report.bin";
        viewModel.Size = 4;
        viewModel.SelectedSizeUnit = SizeUnit.GB;

        viewModel.OnClose();

        Assert.Equal("report.bin", state.FileName);
        Assert.Equal((uint)4, state.Size);
        Assert.Equal(SizeUnit.GB, state.SelectedSizeUnit);
    }

    [Fact]
    public void CheckIfFileValid_FileNameWithOnlyDots_ReturnsExpectedError()
    {
        var result = CreateFileViewModel.ValidateCreateFileOutput(_tempDirectory, "...", 0);

        Assert.False(result.Success);
    }

    [Fact]
    public void CheckIfFileValid_ExistingFile_ReturnsExpectedError()
    {
        var filePath = Path.Combine(_tempDirectory, "existing.txt");
        File.WriteAllText(filePath, "test");

        var result = CreateFileViewModel.ValidateCreateFileOutput(_tempDirectory, "existing.txt", 0);

        Assert.False(result.Success);
    }

    [Fact]
    public void CheckIfFileValid_RelativePath_ReturnsInvalidPathError()
    {
        var result = CreateFileViewModel.ValidateCreateFileOutput("relative", "draft.txt", 0);

        Assert.False(result.Success);
    }

    [Fact]
    public void CheckIfFileValid_FileLargerThanAvailableSpace_ReturnsOutOfMemoryError()
    {
        var filePath = Path.Combine(_tempDirectory, "huge.bin");
        var drive = new DriveInfo(Path.GetPathRoot(filePath)!);
        long sizeBytes = drive.AvailableFreeSpace + 1;

        var result = CreateFileViewModel.ValidateCreateFileOutput(_tempDirectory, "huge.bin", sizeBytes);

        Assert.False(result.Success);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    private static CreateFileViewModel CreateViewModel(CreateFileDialogState? state = null)
    {
        ILogger<CreateFileViewModel> logger = LoggerFactory.Create(_ => { }).CreateLogger<CreateFileViewModel>();
        return new CreateFileViewModel(logger, state ?? new CreateFileDialogState());
    }
}