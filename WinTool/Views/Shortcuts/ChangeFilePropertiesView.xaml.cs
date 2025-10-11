using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class ChangeFilePropertiesView : DialogWindow<ChangeFilePropertiesInput, ChangeFilePropertiesOutput>
{
    public ChangeFilePropertiesView(ChangeFilePropertiesViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
