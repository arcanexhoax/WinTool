using System.ComponentModel;
using System.Windows;
using WinTool.ViewModel;

namespace WinTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
