using WinTool.ViewModel;

namespace WinTool.View;

public partial class RunWithArgsWindow : ModalWindow
{
    public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
    {
        DataContext = runWithArgsVm;
        InitializeComponent();

        textBox.Focus();
    }
}
