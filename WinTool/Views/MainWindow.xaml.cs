using System.ComponentModel;
using System.Windows;
using WinTool.ViewModels;

namespace WinTool.Views;

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
