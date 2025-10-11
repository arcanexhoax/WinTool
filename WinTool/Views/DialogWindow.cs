using System.Windows;
using WinTool.Models;
using WinTool.Native;
using WinTool.ViewModel;

namespace WinTool.Views;

public class ModalWindow : Window
{
    public ModalWindow()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BringProcessToForeground();
        Loaded -= OnLoaded;
    }

    private void BringProcessToForeground()
    {
        INPUT input = new()
        {
            Type = INPUTTYPE.INPUTMOUSE,
            Data = { }
        };
        INPUT[] inputs = [input];

        // Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
        _ = NativeMethods.SendInput(1, inputs, INPUT.Size);
        Activate();
    }
}

public class DialogWindow<TIn, TOut> : ModalWindow
{
    public Result<TOut> ShowDialog(TIn data)
    {
        var vm = (IModalViewModel<TIn, TOut>)DataContext;
        Result<TOut>? result = null;

        SourceInitialized += (_, _) =>
        {
            vm.OnShow(data, r =>
            {
                result = r;
                Close();
            });
        };

        ShowDialog();
        vm.OnClose();

        return result ?? new Result<TOut>(false);
    }
}
