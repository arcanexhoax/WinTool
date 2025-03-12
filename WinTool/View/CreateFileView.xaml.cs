using WinTool.ViewModel;

namespace WinTool.View
{
    public partial class CreateFileView : ModalWindow
    {
        public CreateFileView(CreateFileViewModel createFileViewModel)
        {
            DataContext = createFileViewModel;
            InitializeComponent();

            textBox.Focus();
        }
    }
}
