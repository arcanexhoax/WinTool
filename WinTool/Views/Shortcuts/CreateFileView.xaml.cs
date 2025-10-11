using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class CreateFileView : DialogWindow<string, CreateFileOutput>
{
    public CreateFileView(CreateFileViewModel createFileViewModel)
    {
        DataContext = createFileViewModel;
        InitializeComponent();

        textBox.Focus();
    }
}
