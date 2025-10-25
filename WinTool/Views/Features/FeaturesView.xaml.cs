using System.Windows.Controls;
using WinTool.ViewModels.Features;

namespace WinTool.Views.Features;

public partial class FeaturesView : UserControl
{
    public FeaturesView(FeaturesViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
