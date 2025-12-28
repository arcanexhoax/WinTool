using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public class RunWithArgsWindowBase : DialogWindow<string, string?> { }

public sealed partial class RunWithArgsWindow : RunWithArgsWindowBase
{
    public RunWithArgsWindow(RunWithArgsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // TODO add
        //textBox.Focus();
    }
}
