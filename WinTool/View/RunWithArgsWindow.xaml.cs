using WinTool.ViewModel;

namespace WinTool.View;

public partial class RunWithArgsWindow : DialogWindow<string, string?>
{
    public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
    {
        DataContext = runWithArgsVm;
        InitializeComponent();

        textBox.Focus();
    }
}
