using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace WinTool.View;

public partial class ShortcutsView : UserControl
{
    public ShortcutsView()
    {
        InitializeComponent();
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
        var chars = Path.GetInvalidFileNameChars();

        foreach (char c in chars)
        {
            foreach (var t in e.Text)
            {
                if (t == c)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
