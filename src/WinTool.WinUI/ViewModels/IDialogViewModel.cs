using System;
using WinTool.WinUI.Models;

namespace WinTool.WinUI.ViewModels;

public interface IDialogViewModel<TInput, TOutput>
{
    void OnShow(TInput input, Action<Result<TOutput>> onResult);

    void OnClose();
}
