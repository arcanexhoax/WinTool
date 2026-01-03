using GlobalKeyInterceptor;

namespace WinTool.Models;

public record CreateFileOutput(string FilePath, long Size = 0);

public record RunWithArgsOutput(string? Args, bool RunAsAdmin);

public record EditShortcutInput(Shortcut? Shortcut, string Name);