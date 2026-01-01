using GlobalKeyInterceptor;
using System;

namespace WinTool.Models;

public record CreateFileOutput(string FilePath, long Size = 0);

public record RunWithArgsOutput(string? Args, bool RunAsAdmin);

public record ChangeFilePropertiesInput(string FilePath, bool MediaTagsSupported, string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);

public record ChangeFilePropertiesOutput(string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);

public record EditShortcutInput(Shortcut? Shortcut, string Name);