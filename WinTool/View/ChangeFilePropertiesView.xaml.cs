using System.Windows;
using WinTool.ViewModel;

namespace WinTool.View
{
    public partial class ChangeFilePropertiesView : Window
    {
        public ChangeFilePropertiesView(ChangeFilePropertiesViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
