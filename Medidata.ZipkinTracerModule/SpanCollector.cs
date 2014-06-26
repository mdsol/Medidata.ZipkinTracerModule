using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;

namespace Medidata.ZipkinTracerModule
{
    public class SpanCollector
    {
        private BlockingCollection<Span> spanQueue;

        private const int MAX_QUEUE_SIZE = 100;

        public SpanCollector(IClientProvider clientProvider)
        {
            try
            {
                clientProvider.Setup();

            }
            catch (TException tEx)
            {
                clientProvider.Close();

                throw new Exception("Error setting up connection to scribe", tEx);
            }
            
            spanQueue = new BlockingCollection<Span>(MAX_QUEUE_SIZE);
        }
    }
}
