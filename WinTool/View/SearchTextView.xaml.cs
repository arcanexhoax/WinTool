using System.Windows;
using WinTool.ViewModel;

namespace WinTool.View
{
    /// <summary>
    /// Interaction logic for SearchTextView.xaml
    /// </summary>
    public partial class SearchTextView : Window
    {
        public SearchTextView(SearchTextViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }

        public void ShowFocused()
        {
            Show();
            Activate();
            textBox.Focus();
        }
    }
}
