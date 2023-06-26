using System.Windows;
using WinTool.ViewModel;

namespace WinTool.View
{
    /// <summary>
    /// Interaction logic for RunWithArgsWindow.xaml
    /// </summary>
    public partial class RunWithArgsWindow : Window
    {
        public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
        {
            DataContext = runWithArgsVm;
            InitializeComponent();

            textBox.Focus();
        }
    }
}
