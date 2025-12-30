using WinTool.Models;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class RunWithArgsWindow : DialogWindow<string, RunWithArgsOutput>
{
    public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
    {
        DataContext = runWithArgsVm;
        InitializeComponent();

        textBox.Focus();
    }
}
