using System.Collections.Concurrent;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SpanCollector
    {
        private const int MAX_QUEUE_SIZE = 100;
        internal static BlockingCollection<Span> spanQueue;

        internal SpanProcessor spanProcessor;
        internal IClientProvider clientProvider;

        public SpanCollector(IClientProvider clientProvider, int maxProcessorBatchSize, ILog logger)
        {
            if ( spanQueue == null)
            {
                spanQueue = new BlockingCollection<Span>(MAX_QUEUE_SIZE);
            }

            this.clientProvider = clientProvider;

            spanProcessor = new SpanProcessor(spanQueue, clientProvider, maxProcessorBatchSize, logger);
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
