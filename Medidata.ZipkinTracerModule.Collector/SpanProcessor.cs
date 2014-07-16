using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class SpanProcessor
    {
        private List<LogEntry> logEntries;
        private TBinaryProtocol.Factory protocolFactory;

        private const int WAIT_INTERVAL_TO_DEQUEUE_MS = 1000;

        private int MAX_BATCH_SIZE = 20;
        private BlockingCollection<Span> spanQueue;
        private IClientProvider clientProvider;

        internal CancellationTokenSource cancellationTokenSource;
        internal SpanProcessorTaskFactory spanProcessorTaskFactory;
        private readonly BlockingCollection<Span> queue;

        private int subsequentEmptyQueueCount;

        public SpanProcessor(BlockingCollection<Span> spanQueue, IClientProvider clientProvider)
        {
            if ( spanQueue == null) 
            {
                throw new ArgumentNullException("spanQueue is null");
            }

            if ( clientProvider == null) 
            {
                throw new ArgumentNullException("clientProvider is null");
            }

            this.spanQueue = spanQueue;
            this.clientProvider = clientProvider;
            logEntries = new List<LogEntry>(MAX_BATCH_SIZE);
            protocolFactory = new TBinaryProtocol.Factory();
            spanProcessorTaskFactory = new SpanProcessorTaskFactory();
        }

        public virtual void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        public virtual void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            spanProcessorTaskFactory.CreateAndStart(() => LogSubmittedSpans(), cancellationTokenSource);
        }

        internal void LogSubmittedSpans()
        {
            while(!cancellationTokenSource.Token.IsCancellationRequested )
            {
                Span span;
                queue.TryTake(out span, WAIT_INTERVAL_TO_DEQUEUE_MS);
                if (span != null)
                {
                    logEntries.Add(Create(span));
                    subsequentEmptyQueueCount = 0;
                }
                else
                {
                    subsequentEmptyQueueCount++;
                }

                //if (logEntries.Count() >= MAX_BATCH_SIZE 
                //    || logEntries.Any() && cancellationTokenSource.Token.IsCancellationRequested
                //    || logEntries.Any() && subsequentEmptyQueueCount > MAX_SUBSEQUENT_EMPTY_QUEUE)
                if ( logEntries.Any() )
                {
                    Log(clientProvider, logEntries);
                    logEntries.Clear();
                    subsequentEmptyQueueCount = 0;
                }
            } 
        }

        private void Log(IClientProvider client, List<LogEntry> logEntries)
        {
            try
            {
                clientProvider.Log(logEntries);
            }
            catch (TException tEx)
            {
                throw new Exception("Error writing to scribe", tEx);
            }
        }

        private LogEntry Create(Span span)
        {
            var spanAsString = Convert.ToBase64String(ConvertSpanToBytes(span));
            return new LogEntry()
            {
                Category = "zipkin",
                Message = spanAsString
            };
        }

        private byte[] ConvertSpanToBytes(Span span)
        {
            var buf = new MemoryStream();
            TProtocol protocol = protocolFactory.GetProtocol(new TStreamTransport(buf, buf));
            span.Write(protocol);
            return buf.ToArray();
        }
    }
}
