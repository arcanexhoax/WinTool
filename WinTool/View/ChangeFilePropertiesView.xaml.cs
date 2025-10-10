using WinTool.Model;
using WinTool.ViewModel;

namespace WinTool.View;

public partial class ChangeFilePropertiesView : DialogWindow<ChangeFilePropertiesInput, ChangeFilePropertiesOutput>
{
    public ChangeFilePropertiesView(ChangeFilePropertiesViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
