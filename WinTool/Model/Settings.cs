namespace WinTool.Model
{
    public class Settings
    {
        public bool EnableSwitchLanguagePopup { get; set; }
        public bool WindowsStartupEnabled { get; set; }
        public string? NewFileTemplate { get; set; }

        public Settings()
        {
            NewFileTemplate = "NewFile.txt";
        }
    }
}
