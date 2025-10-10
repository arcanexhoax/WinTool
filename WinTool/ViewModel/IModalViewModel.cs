using System;
using WinTool.Model;

namespace WinTool.ViewModel;

public interface IModalViewModel<TInput, TOutput>
{
    void OnShow(TInput input, Action<Result<TOutput>> onResult);

    void OnClose();
}
