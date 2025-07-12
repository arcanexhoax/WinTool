namespace WinTool.Model
{
    // TODO add it to oter result classes
    public record Result(bool Success, string? Message = null);

    public record Result<T>(bool Success, T? Data = default, string? Message = null) : Result(Success, Message);

    public record CreateFileResult(bool Success, string? FilePath, long Size = 0);

    public record RunWithArgsResult(bool Success, string? Args);
}
