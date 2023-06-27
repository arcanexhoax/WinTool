namespace WinTool.Model
{
    public class CreateFileResult
    {
        public bool Success { get; }
        public string? FilePath { get; }

        public CreateFileResult(bool success, string? filePath)
        {
            Success = success;
            FilePath = filePath;
        }
    }
}
