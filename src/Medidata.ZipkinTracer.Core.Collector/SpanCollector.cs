using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SpanCollector
    {
        private const int MAX_QUEUE_SIZE = 100;
        internal static BlockingCollection<Span> spanQueue = new BlockingCollection<Span>(MAX_QUEUE_SIZE);

        internal SpanProcessor spanProcessor;
        internal IClientProvider clientProvider;

        public SpanCollector(IClientProvider clientProvider, int maxProcessorBatchSize)
        {
            this.clientProvider = clientProvider;

            try
            {
                clientProvider.Setup();
            }
            catch (TException tEx)
            {
                clientProvider.Close();
                throw tEx;
            }
            
            spanProcessor = new SpanProcessor(spanQueue, clientProvider, maxProcessorBatchSize);
        }

        public virtual void Collect(Span span)
        {
            spanQueue.Add(span);
        }

        public virtual void Start()
        {
            spanProcessor.Start();
        }

        public virtual void Stop()
        {
            spanProcessor.Stop();
            clientProvider.Close();
        }
    }
}
