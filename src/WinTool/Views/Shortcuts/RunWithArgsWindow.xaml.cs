using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class RunWithArgsWindow : DialogWindow<string, string?>
{
    public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
    {
        DataContext = runWithArgsVm;
        InitializeComponent();

        textBox.Focus();
    }
}
