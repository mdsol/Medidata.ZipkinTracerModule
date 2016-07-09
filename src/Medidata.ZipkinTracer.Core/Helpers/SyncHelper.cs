using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
