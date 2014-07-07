﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;

namespace Medidata.ZipkinTracerModule
{
    public class SpanCollector : ISpanCollector
    {
        internal BlockingCollection<Span> spanQueue;

        private const int MAX_QUEUE_SIZE = 100;
        internal ISpanProcessor spanProcessor;
        private IClientProvider clientProvider;

        public SpanCollector(IClientProvider clientProvider)
        {
            this.clientProvider = clientProvider;

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
            spanProcessor = new SpanProcessor(spanQueue, clientProvider);
        }

        public void Collect(Span span)
        {
            spanQueue.Add(span);
        }

        public void Start()
        {
            spanProcessor.Start();
        }

        public void Stop()
        {
            spanProcessor.Stop();
            clientProvider.Close();
        }
    }
}
