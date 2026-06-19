using GlobalKeyInterceptor;

namespace WinTool.Models;

public record RunWithArgsOutput(string? Args, bool RunAsAdmin);

public record EditShortcutInput(Shortcut? Shortcut, string Name);
