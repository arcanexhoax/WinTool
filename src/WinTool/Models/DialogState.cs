using WinTool.ViewModels.Shortcuts;

namespace WinTool.Models;

public class CreateFileDialogState
{
    public string? FileName { get; set; }

    public uint Size { get; set; }

    public SizeUnit SelectedSizeUnit { get; set; } = SizeUnit.B;
}

public class RunWithArgsDialogState
{
    public string? Args { get; set; }

    public bool RunAsAdmin { get; set; }
}