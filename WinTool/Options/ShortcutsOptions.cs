namespace WinTool.Options;

public class ShortcutsOptions
{
    public ShortcutOptions CreateFile { get; set; } = new() { Shortcut = "Ctrl + E" };
    public FastFileCreationOptions FastFileCreation { get; set; } = new() { Shortcut = "Ctrl + Shift + E" };
    public ShortcutOptions SelectedItemCopyPath { get; set; } = new() { Shortcut = "Ctrl + Shift + C" };
    public ShortcutOptions SelectedItemCopyName { get; set; } = new() { Shortcut = "Ctrl + Shift + X" };
    public ShortcutOptions RunWithArgs { get; set; } = new() { Shortcut = "Ctrl + O" };
    public ShortcutOptions OpenFolderInCmd { get; set; } = new() { Shortcut = "Ctrl + Shift + L" };
    public ShortcutOptions ChangeFileProperties { get; set; } = new() { Shortcut = "Ctrl + F2" };
}

public class ShortcutOptions
{
    public required string Shortcut { get; set; } 
}

public class FastFileCreationOptions : ShortcutOptions
{
    public string NewFileTemplate { get; set; } = "NewFile.txt";
}