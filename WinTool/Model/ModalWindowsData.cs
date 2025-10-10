using System;
using WinTool.ViewModel;

namespace WinTool.Model;

public record CreateFileInput(string FileName, uint Size, SizeUnit SizeUnit);

public record ChangeFilePropertiesInput(string FilePath, bool MediaTagsSupported, string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);

public record ChangeFilePropertiesOutput(string? Title, string[]? Performers, string? Album, string[]? Genres, string? Lyrics, uint Year, DateTime CreationTime, DateTime ChangeTime);
