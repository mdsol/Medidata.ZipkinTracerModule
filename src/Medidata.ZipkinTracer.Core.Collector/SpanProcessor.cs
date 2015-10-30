using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SpanProcessor
    {
        //send contents of queue if it has pending items but less than max batch size after doing max number of polls
        internal const int MAX_NUMBER_OF_POLLS = 5;

        private readonly Uri uri;
        internal BlockingCollection<Span> spanQueue;
        internal ConcurrentQueue<SerializableSpan> serializableSpans; 

        internal SpanProcessorTaskFactory spanProcessorTaskFactory;
        internal int subsequentPollCount;
        internal int retries;
        internal int maxBatchSize;
        private readonly ILog logger;

        public SpanProcessor(Uri uri, BlockingCollection<Span> spanQueue, int maxBatchSize, ILog logger)
        {
            if ( spanQueue == null) 
            {
                throw new ArgumentNullException("spanQueue is null");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("zipkin Uri is uri");
            }

            this.uri = uri;
            this.spanQueue = spanQueue;
            this.serializableSpans = new ConcurrentQueue<SerializableSpan>();
            this.maxBatchSize = maxBatchSize;
            this.logger = logger;
            spanProcessorTaskFactory = new SpanProcessorTaskFactory(logger);
        }

        public virtual void Stop()
        {
            spanProcessorTaskFactory.StopTask();
            LogSubmittedSpans();
        }

        public virtual void Start()
        {
            spanProcessorTaskFactory.CreateAndStart(LogSubmittedSpans);
        }

        internal virtual void LogSubmittedSpans()
        {
            Span span;
            while (spanQueue.TryTake(out span))
            {
                serializableSpans.Enqueue(new SerializableSpan(span));
                subsequentPollCount = 0;
            }
            if(serializableSpans.Count > 0) subsequentPollCount++;


            if ((serializableSpans.Count() >= maxBatchSize)
                || (serializableSpans.Any() && spanProcessorTaskFactory.IsTaskCancelled())
                || (subsequentPollCount > MAX_NUMBER_OF_POLLS))
            {
                try
                {
                    if (serializableSpans.Any()) 
                        SendSpansToZipkin(JsonConvert.SerializeObject(serializableSpans.ToList()));
                }
                catch (WebException ex)
                {
                    logger.Error("Failed to send to zipking with error: " + ex.Message);
                }
            }
        }

        public virtual void SendSpansToZipkin(string requestBody)
        {
            using (var client = new WebClient())
            {
                const string tracerPath = "/api/v1/spans";

                try
                {
                    client.BaseAddress = uri.ToString();
                    client.UploadString(tracerPath, "POST", requestBody);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if ((response != null))
                    {
                        // read all response info
                        var responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            var responseString = new StreamReader(responseStream).ReadToEnd();
                            logger.Error("Sending Spans to Failed with error: " + ex.Message + " and message:" + responseString);
                        }
                    }
                }
            }
        }
    }
}
