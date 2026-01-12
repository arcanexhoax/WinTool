using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinTool.Services;

public class StaThreadService : IDisposable
{
    private const int OperationTimeoutMs = 5000;

    private readonly Thread _staThread;
    private readonly BlockingCollection<Action> _queue = [];
    private readonly AutoResetEvent _initEvent = new(false);

    public StaThreadService()
    {
        _staThread = new Thread(Loop);
        _staThread.SetApartmentState(ApartmentState.STA);
        _staThread.IsBackground = true;
        _staThread.Start();

        _initEvent.WaitOne();
    }

    private void Loop()
    {
        _initEvent.Set();

        foreach (var command in _queue.GetConsumingEnumerable())
        {
            command();
        }
    }

    public void Invoke(Action action)
    {
        Invoke(() =>
        {
            action();
            return true;
        });
    }

    public T Invoke<T>(Func<T> func)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            return func();

        T? result = default;
        Exception? exception = null;

        using var done = new ManualResetEventSlim(false);

        _queue.Add(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally 
            { 
                done.Set(); 
            }
        });

        if (!done.Wait(OperationTimeoutMs))
            throw new TimeoutException($"STA operation timed out after {OperationTimeoutMs} ms.");

        if (exception is not null)
            throw exception;

        return result!;
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
    }
}

