using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class CreateFileWindow : DialogWindow<string, CreateFileOutput>
{
    public CreateFileWindow(CreateFileViewModel createFileViewModel)
    {
        DataContext = createFileViewModel;
        InitializeComponent();

        textBox.Focus();
    }
}
