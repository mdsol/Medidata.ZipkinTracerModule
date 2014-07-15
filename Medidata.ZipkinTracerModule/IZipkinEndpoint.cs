using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medidata.ZipkinTracerModule
{
    public interface IZipkinEndpoint
    {
        Endpoint GetEndpoint(string serviceName);
    }
}
