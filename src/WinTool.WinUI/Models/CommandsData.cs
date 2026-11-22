using GlobalKeyInterceptor;

namespace WinTool.WinUI.Models;

public record RunWithArgsOutput(string? Args, bool RunAsAdmin, bool KeepConsoleOpen);

public record EditShortcutInput(Shortcut? Shortcut, string Name);
