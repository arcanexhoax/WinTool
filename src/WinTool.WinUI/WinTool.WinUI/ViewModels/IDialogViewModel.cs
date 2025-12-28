using System;
using WinTool.Models;

namespace WinTool.ViewModels;

public interface IDialogViewModel<TInput, TOutput>
{
    void OnShow(TInput input, Action<Result<TOutput>> onResult);

    void OnClose();
}
