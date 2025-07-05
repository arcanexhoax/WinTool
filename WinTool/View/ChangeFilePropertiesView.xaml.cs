using WinTool.ViewModel;

namespace WinTool.View;

public partial class ChangeFilePropertiesView : ModalWindow
{
    public ChangeFilePropertiesView(ChangeFilePropertiesViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
