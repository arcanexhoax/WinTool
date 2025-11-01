using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class ChangeFilePropertiesWindow : DialogWindow<ChangeFilePropertiesInput, ChangeFilePropertiesOutput>
{
    public ChangeFilePropertiesWindow(ChangeFilePropertiesViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
