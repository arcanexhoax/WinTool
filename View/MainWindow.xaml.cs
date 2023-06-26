using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
            DataContext = new MainViewModel(this);
            InitializeComponent();
        }

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
}
