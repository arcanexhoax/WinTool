namespace WinTool.Model;

public record Result(bool Success, string? Message = null);

public record Result<T>(bool Success, T? Data = default, string? Message = null) : Result(Success, Message);
