using Windows.ApplicationModel.DataTransfer;

namespace WinTool.Extensions;

public static class ClipboardExtensions
{
    extension(Clipboard)
    {
        public static void SetText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
