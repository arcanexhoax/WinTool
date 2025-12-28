using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public class CreateFileWindowBase : DialogWindow<string, CreateFileOutput> { }

public sealed partial class CreateFileWindow : CreateFileWindowBase
{
    public CreateFileWindow(CreateFileViewModel createFileViewModel)
    {
        InitializeComponent();
        DataContext = createFileViewModel;

        // TODO add 
        // textBox.Focus();
    }
}
