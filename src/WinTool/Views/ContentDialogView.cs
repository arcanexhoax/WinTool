using System.Windows.Controls;
using System.Windows.Threading;
using WinTool.Models;
using WinTool.ViewModels;

namespace WinTool.Views;

public class ContentDialogView<TIn, TOut> : UserControl, IDialog<TIn, TOut>
{
    public Result<TOut> ShowDialog(TIn input)
    {
        if (ContentDialogHost.Current is not { } host)
            return new Result<TOut>(false);

        var vm = (IDialogViewModel<TIn, TOut>)DataContext;
        var frame = new DispatcherFrame();
        Result<TOut>? result = null;

        vm.OnShow(input, r =>
        {
            result = r;
            host.Close();
            frame.Continue = false;
        });

        host.Show(this);
        Dispatcher.PushFrame(frame);
        vm.OnClose();

        return result ?? new Result<TOut>(false);
    }
}
