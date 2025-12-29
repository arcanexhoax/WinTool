using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Features;
using WinTool.WinUI;

namespace WinTool.Views.Features;

public sealed partial class FeaturesPage : Page
{
    public FeaturesPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<FeaturesViewModel>();
    }
}
