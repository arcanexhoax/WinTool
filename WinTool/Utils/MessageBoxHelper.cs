using System.Windows;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.Utils
{
    public class MessageBoxHelper
    {
        public static void ShowError(string? message)
        {
            MessageBox.Show(message, Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
