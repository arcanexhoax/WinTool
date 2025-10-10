using WinTool.Model;
using WinTool.ViewModel;

namespace WinTool.View;

public partial class CreateFileView : DialogWindow<string, CreateFileOutput>
{
    public CreateFileView(CreateFileViewModel createFileViewModel)
    {
        DataContext = createFileViewModel;
        InitializeComponent();

        textBox.Focus();
    }
}
