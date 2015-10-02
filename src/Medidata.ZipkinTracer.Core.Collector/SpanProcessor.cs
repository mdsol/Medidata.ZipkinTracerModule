using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SpanProcessor
    {
        //send contents of queue if it has pending items but less than max batch size after doing max number of polls
        internal const int MAX_NUMBER_OF_POLLS = 5;

        private TBinaryProtocol.Factory protocolFactory;
        internal BlockingCollection<Span> spanQueue;
        private IClientProvider clientProvider;

        internal List<LogEntry> logEntries;
        internal SpanProcessorTaskFactory spanProcessorTaskFactory;
        internal int subsequentPollCount;
        internal int retries;
        internal int maxBatchSize;

        public SpanProcessor(BlockingCollection<Span> spanQueue, IClientProvider clientProvider, int maxBatchSize, ILog logger)
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
            spanProcessorTaskFactory = new SpanProcessorTaskFactory(logger);
        }

        public virtual void Stop()
        {
            spanProcessorTaskFactory.StopTask();
            LogSubmittedSpans();
        }

        public virtual void Start()
        {
            spanProcessorTaskFactory.CreateAndStart(() => LogSubmittedSpans());
        }

        internal virtual void LogSubmittedSpans()
        {
            Span span;
            spanQueue.TryTake(out span);
            if (span != null)
            {
                logEntries.Add(Create(span));
                subsequentPollCount = 0;
            }
            else if (logEntries.Any())
            {
                subsequentPollCount++;
            }

            if ((logEntries.Count() >= maxBatchSize)
                || (logEntries.Any() && spanProcessorTaskFactory.IsTaskCancelled())
                || (subsequentPollCount > MAX_NUMBER_OF_POLLS))
            {
                var entries = logEntries;
                logEntries = new List<LogEntry>();
                subsequentPollCount = 0;
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
            catch (TException)
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
