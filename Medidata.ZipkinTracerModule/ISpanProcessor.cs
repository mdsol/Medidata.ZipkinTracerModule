using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public interface ISpanProcessor
    {
        void Start();
        void Stop();
    }
}
