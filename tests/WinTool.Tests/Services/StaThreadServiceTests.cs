using WinTool.Services;

namespace WinTool.Tests.Services;

public class StaThreadServiceTests
{
    [Fact]
    public void Invoke_FromMtaThread_UsesDedicatedStaThread()
    {
        using var service = new StaThreadService();

        var result = service.Invoke(() => (Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.GetApartmentState()));

        Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, result.ManagedThreadId);
        Assert.Equal(ApartmentState.STA, result.Item2);
    }

    [Fact]
    public void Invoke_WhenCallbackThrows_RethrowsOriginalException()
    {
        using var service = new StaThreadService();

        var exception = Assert.Throws<InvalidOperationException>(() => service.Invoke<int>(() => throw new InvalidOperationException("boom")));

        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public void Invoke_FromStaThread_ExecutesInlineOnCallerThread()
    {
        using var service = new StaThreadService();
        Exception? exception = null;
        int callerThreadId = 0;
        int callbackThreadId = 0;

        var thread = new Thread(() =>
        {
            try
            {
                callerThreadId = Thread.CurrentThread.ManagedThreadId;
                callbackThreadId = service.Invoke(() => Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.NotEqual(0, callerThreadId);
        Assert.Equal(callerThreadId, callbackThreadId);
    }

    [Fact]
    public void Dispose_PreventsFurtherScheduling()
    {
        var service = new StaThreadService();
        service.Dispose();

        Assert.Throws<InvalidOperationException>(() => service.Invoke(() => 42));
    }
}