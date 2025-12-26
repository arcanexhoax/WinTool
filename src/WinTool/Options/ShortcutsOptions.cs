using System.Collections.Generic;

namespace WinTool.Options;

public class ShortcutsOptions
{
    public Dictionary<string, string> Shortcuts { get; set; } = [];
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