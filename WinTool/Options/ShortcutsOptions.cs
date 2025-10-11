using System.Collections.Generic;

namespace WinTool.Options;

public class ShortcutsOptions
{
    public Dictionary<string, string> Shortcuts { get; set; } = new()
    {
        { ShortcutNames.CreateFile, "Ctrl + E" },
        { ShortcutNames.FastFileCreation, "Ctrl + Shift + E" },
        { ShortcutNames.SelectedItemCopyPath, "Ctrl + Shift + C" },
        { ShortcutNames.SelectedItemCopyName, "Ctrl + Shift + X" },
        { ShortcutNames.RunWithArgs, "Ctrl + O" },
        { ShortcutNames.OpenFolderInCmd, "Ctrl + Shift + L" },
        { ShortcutNames.ChangeFileProperties, "Ctrl + F2" }
    };
}

public class ShortcutNames
{
    public const string CreateFile = nameof(CreateFile);
    public const string FastFileCreation = nameof(FastFileCreation);
    public const string SelectedItemCopyPath = nameof(SelectedItemCopyPath);
    public const string SelectedItemCopyName = nameof(SelectedItemCopyName);
    public const string RunWithArgs = nameof(RunWithArgs);
    public const string OpenFolderInCmd = nameof(OpenFolderInCmd);
    public const string ChangeFileProperties = nameof(ChangeFileProperties);
}