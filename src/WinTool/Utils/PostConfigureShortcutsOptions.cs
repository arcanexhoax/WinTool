using GlobalKeyInterceptor;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using WinTool.Extensions;
using WinTool.Options;

namespace WinTool.Utils;

public class PostConfigureShortcutsOptions : IPostConfigureOptions<ShortcutsOptions>
{
    private readonly Dictionary<string, string> _defaultShortcuts = new()
    {
        { ShortcutNames.CreateFile, "Ctrl + E" },
        { ShortcutNames.FastFileCreation, "Ctrl + Shift + E" },
        { ShortcutNames.SelectedItemCopyPath, "Ctrl + Shift + C" },
        { ShortcutNames.SelectedItemCopyName, "Ctrl + Shift + X" },
        { ShortcutNames.RunWithArgs, "Ctrl + O" },
        { ShortcutNames.OpenFolderInCmd, "Ctrl + P" },
        { ShortcutNames.OpenFolderInCmdAsAdmin, "Ctrl + Shift + P" },
    };

    public void PostConfigure(string? _, ShortcutsOptions options)
    {
        foreach (var (name, shortcutStr) in _defaultShortcuts)
        {
            if (!options.Shortcuts.TryGetValue(name, out var actualShortcutStr) || !Shortcut.IsValid(actualShortcutStr))
                options.Shortcuts[name] = shortcutStr;
        }
    }
}
