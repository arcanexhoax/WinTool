namespace WinTool.Model
{
    public class CreateFileResult
    {
        public bool Success { get; }
        public string? FilePath { get; }
        public long Size { get; }

        public CreateFileResult(bool success, string? filePath, long size = 0)
        {
            Success = success;
            FilePath = filePath;
            Size = size;
        }
    }
}
