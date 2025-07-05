using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WinTool.ViewModel;

namespace WinTool;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        mainViewModel.ShowWindowRequested += (_, _) => Show();
        DataContext = mainViewModel;
        InitializeComponent();
    }

    private void OnWindowActivated(object? sender, System.EventArgs e) => Show();

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
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
