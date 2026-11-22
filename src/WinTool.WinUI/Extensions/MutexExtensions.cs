using System.Threading;

namespace WinTool.WinUI.Extensions;

internal static class MutexExtensions
{
    private static Mutex? s_secondInstanceMutex;

    extension(Mutex)
    {
        public static bool TryAttachAsFirstInstance()
        {
            s_secondInstanceMutex = new Mutex(true, "WinTool-10fdf33711f4591a368bd6a0b0e20cc1", out bool isFirstInstance);
            return isFirstInstance;
        }

        public static void Release()
        {
            s_secondInstanceMutex?.Dispose();
            s_secondInstanceMutex = null;
        }
    }
}
