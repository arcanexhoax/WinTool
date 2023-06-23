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

            textBox.Focus();
        }
    }
}
