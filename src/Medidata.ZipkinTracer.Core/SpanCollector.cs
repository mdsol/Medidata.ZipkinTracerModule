using System;
using System.Collections.Concurrent;
using log4net;
using Medidata.ZipkinTracer.Models;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanCollector
    {
        private const int MAX_QUEUE_SIZE = 100;
        internal static BlockingCollection<Span> spanQueue;

        internal SpanProcessor spanProcessor;

        private static SpanCollector instance;

        public static SpanCollector GetInstance(Uri uri, uint maxProcessorBatchSize, ILog logger)
        {
            if (instance == null)
            {
                instance = new SpanCollector(uri, maxProcessorBatchSize, logger);
                instance.Start();
            }
            return instance;
        }

        public SpanCollector(Uri uri, uint maxProcessorBatchSize, ILog logger)
        {
            if ( spanQueue == null)
            {
                spanQueue = new BlockingCollection<Span>(MAX_QUEUE_SIZE);
            }

            spanProcessor = new SpanProcessor(uri, spanQueue, maxProcessorBatchSize, logger);
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
        }
    }
}
