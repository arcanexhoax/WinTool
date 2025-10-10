using System;

namespace WinTool.Model;

public record CreateFileOutput(string FilePath, long Size = 0);

public record ChangeFilePropertiesInput(string FilePath, bool MediaTagsSupported, string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);

public record ChangeFilePropertiesOutput(string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);
