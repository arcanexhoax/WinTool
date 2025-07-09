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
}
