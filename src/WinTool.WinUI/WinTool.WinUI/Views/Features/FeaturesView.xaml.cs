using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Features;

namespace WinTool.Views.Features;

public sealed partial class FeaturesView : UserControl
{
    public FeaturesView(FeaturesViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
