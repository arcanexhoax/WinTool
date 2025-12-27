using GlobalKeyInterceptor;
using System;

namespace WinTool.Models;

public class ShortcutContext
{
    public bool IsEditing { get; set; }
}

public record ShortcutCommand(string Id, Shortcut? Shortcut, Action Command, string Icon, string Description)
{
    public Shortcut? Shortcut { get; set; } = Shortcut;
}