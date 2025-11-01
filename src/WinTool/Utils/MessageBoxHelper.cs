using System.Windows;
using WinTool.Properties;

namespace WinTool.Utils;

public class MessageBoxHelper
{
    public static void ShowError(string? message)
    {
        MessageBox.Show(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
