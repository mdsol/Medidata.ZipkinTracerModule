using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class SpanProcessor
    {
        //wait time to poll for dequeuing
        private const int WAIT_INTERVAL_TO_DEQUEUE_MS = 1000;

        //send contents of queue if it has been empty for number of polls
        internal const int MAX_SUBSEQUENT_EMPTY_QUEUE = 5;

        private TBinaryProtocol.Factory protocolFactory;
        internal BlockingCollection<Span> spanQueue;
        private IClientProvider clientProvider;

        internal List<LogEntry> logEntries;
        internal CancellationTokenSource cancellationTokenSource;
        internal SpanProcessorTaskFactory spanProcessorTaskFactory;
        internal int subsequentEmptyQueueCount;
        internal int retries;
        internal int maxBatchSize;

        public SpanProcessor(BlockingCollection<Span> spanQueue, IClientProvider clientProvider, int maxBatchSize)
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
            this.maxBatchSize = maxBatchSize;
            logEntries = new List<LogEntry>();
            protocolFactory = new TBinaryProtocol.Factory();
            spanProcessorTaskFactory = new SpanProcessorTaskFactory();
        }

        public virtual void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                LogSubmittedSpans();
            }
        }

        public virtual void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            spanProcessorTaskFactory.CreateAndStart(() => LogSubmittedSpansWrapper(), cancellationTokenSource);
        }

        internal async void LogSubmittedSpansWrapper()
        {
            while(!cancellationTokenSource.Token.IsCancellationRequested )
            {
                LogSubmittedSpans();
                await Task.Delay(500, cancellationTokenSource.Token);
            } 
        }

        internal void LogSubmittedSpans()
        {
            Span span;
            spanQueue.TryTake(out span);
            if (span != null)
            {
                logEntries.Add(Create(span));
                subsequentEmptyQueueCount = 0;
            }
            else
            {
                subsequentEmptyQueueCount++;
            }

            if (logEntries.Count() >= maxBatchSize
                || logEntries.Any() && cancellationTokenSource.Token.IsCancellationRequested
                || logEntries.Any() && subsequentEmptyQueueCount > MAX_SUBSEQUENT_EMPTY_QUEUE)
            {
                var entries = logEntries;
                logEntries = new List<LogEntry>();
                subsequentEmptyQueueCount = 0;
                Log(clientProvider, entries);
            }
        }

        internal void Log(IClientProvider client, List<LogEntry> logEntries)
        {
            try
            {
                clientProvider.Log(logEntries);
                retries = 0;
            }
            catch (TException tEx)
            {
                if ( retries < 3 )
                {
                    retries++;
                    Log(client, logEntries);
                }
                else
                {
                    throw;
                }
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
