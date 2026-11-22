using GlobalKeyInterceptor;
using System;

namespace WinTool.WinUI.Models;

public class ShortcutContext
{
    public bool IsEditing { get; set; }
}

public record ShortcutCommand(string Id, Shortcut? Shortcut, Action Command)
{
    public Shortcut? Shortcut { get; set; } = Shortcut;
}
