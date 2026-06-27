using WinTool.Models;

namespace WinTool.Views;

public interface IDialog<TIn, TOut>
{
    Result<TOut> ShowDialog(TIn input);
}
