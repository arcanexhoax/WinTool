using System.Windows;
using WinTool.ViewModel;

namespace WinTool.View
{
    /// <summary>
    /// Interaction logic for SearchTextResultView.xaml
    /// </summary>
    public partial class SearchTextResultView : Window
    {
        public SearchTextResultView(SearchTextResultViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
