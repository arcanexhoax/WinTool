using System.Windows;
using WinTool.Properties;

namespace WinTool.Extensions;

public static class MessageBoxExtensions
{
    extension(MessageBox)
    {
        public static void ShowError(string? message)
        {
            MessageBox.Show(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
