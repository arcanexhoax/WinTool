using System.Windows;
using WinTool.ViewModel;

namespace WinTool.View
{
    public partial class CreateFileView : Window
    {
        public CreateFileView(CreateFileViewModel createFileViewModel)
        {
            DataContext = createFileViewModel;
            InitializeComponent();
        }

        public void ShowFocused()
        {
            Show();
            Activate();
            textBox.Focus();
        }
    }
}
