using System;

namespace Medidata.ZipkinTracer.Core.Helpers
{
    public static class SyncHelper
    {
        public static void ExecuteSafely(object sync, Func<bool> canExecute, Action actiontoExecuteSafely)
        {
            if (canExecute())
            {
                lock (sync)
                {
                    if (canExecute())
                    {
                        actiontoExecuteSafely();
                    }
                }
            }
        }
    }
}
