using GlobalKeyInterceptor;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using WinTool.Extensions;

namespace WinTool.Options;

public class PostConfigureShortcutsOptions : IPostConfigureOptions<ShortcutsOptions>
{
    private readonly Dictionary<string, string> _defaultShortcuts = new()
    {
        { ShortcutNames.CreateFile, "Ctrl + E" },
        { ShortcutNames.FastFileCreation, "Ctrl + Shift + E" },
        { ShortcutNames.SelectedItemCopyPath, "Ctrl + Shift + C" },
        { ShortcutNames.SelectedItemCopyName, "Ctrl + Shift + X" },
        { ShortcutNames.RunFileAsAdmin, "Ctrl + Shift + Enter" },
        { ShortcutNames.RunFileWithArgs, "Ctrl + O" },
        { ShortcutNames.OpenFolderInCmd, "Ctrl + P" },
        { ShortcutNames.OpenFolderInCmdAsAdmin, "Ctrl + Shift + P" },
    };

    public void PostConfigure(string? _, ShortcutsOptions o)
    {
        var usedShortcuts = new HashSet<Shortcut>();
        var acceptedShortcuts = new HashSet<string>();
        var defaultShortcuts = _defaultShortcuts.ToDictionary(s => s.Key, s => Shortcut.Parse(s.Value)!);
        var actualShortcuts = o.Shortcuts.ToDictionary(
            s => s.Key,
            s => Shortcut.Parse(s.Value) is { Modifier: not KeyModifier.None } p ? p : null);

        foreach (var (name, actualSc) in actualShortcuts)
        {
            if (defaultShortcuts.ContainsKey(name) && actualSc != null && usedShortcuts.Add(actualSc))
                acceptedShortcuts.Add(name);
            else
                o.Shortcuts.Remove(name);
        }

        foreach (var (name, defaultSc) in defaultShortcuts)
        {
            if (!acceptedShortcuts.Contains(name))
                o.Shortcuts[name] = usedShortcuts.Add(defaultSc) ? _defaultShortcuts[name] : null;
        }
    }
}
