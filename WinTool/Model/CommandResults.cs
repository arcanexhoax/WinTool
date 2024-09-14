namespace WinTool.Model
{
    public record CreateFileResult(bool Success, string? FilePath, CreateFileData? CreateFileData, long Size = 0);

    public record RunWithArgsResult(bool Success, string? Args);
}
