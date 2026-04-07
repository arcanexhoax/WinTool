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

public enum SizeUnit : long
{
    B = 1,
    KB = 1024,
    MB = 1_048_576,
    GB = 1_073_741_824,
    TB = 1_099_511_627_776,
}