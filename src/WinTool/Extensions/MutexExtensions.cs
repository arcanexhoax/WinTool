using System.Threading;
using WinTool.Native;

namespace WinTool.Extensions;

internal static class MutexExtensions
{
    private static Mutex? _secondInstanceMutex;

    extension(Mutex)
    {
        public static bool TryAttachAsFirstInstance()
        {
            _secondInstanceMutex = new Mutex(true, "WinTool-10fdf33711f4591a368bd6a0b0e20cc1", out bool isFirstInstance);
            return isFirstInstance;
        }

        public static void Release()
        {
            _secondInstanceMutex?.Dispose();
            _secondInstanceMutex = null;
        }
    }
}
