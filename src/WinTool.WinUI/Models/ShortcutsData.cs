using GlobalKeyInterceptor;
using System;
using System.Threading.Tasks;

namespace WinTool.WinUI.Models;

public class ShortcutContext
{
    public bool IsEditing { get; set; }
}

public record ShortcutCommand(string Id, Shortcut? Shortcut, Func<Task> Command)
{
    public Shortcut? Shortcut { get; set; } = Shortcut;
}
