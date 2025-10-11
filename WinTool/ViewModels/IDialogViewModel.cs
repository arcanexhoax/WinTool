using System;
using WinTool.Models;

namespace WinTool.ViewModel;

public interface IDialogViewModel<TInput, TOutput>
{
    void OnShow(TInput input, Action<Result<TOutput>> onResult);

    void OnClose();
}
