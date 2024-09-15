namespace WinTool.Model
{
    public record CreateFileResult(bool Success, string? FilePath, long Size = 0);

    public record RunWithArgsResult(bool Success, string? Args);
}
