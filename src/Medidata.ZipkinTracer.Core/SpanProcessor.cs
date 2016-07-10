using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Models;
using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanProcessor
    {
        //send contents of queue if it has pending items but less than max batch size after doing max number of polls
        internal const int MAX_NUMBER_OF_POLLS = 5;
        internal const string ZIPKIN_SPAN_POST_PATH = "/api/v1/spans";

        private readonly Uri uri;
        internal BlockingCollection<Span> spanQueue;

        //using a queue because even as we pop items to send to zipkin, another 
        //thread can be adding spans if someone shares the span processor accross threads
        internal ConcurrentQueue<JsonSpan> serializableSpans; 
        internal SpanProcessorTaskFactory spanProcessorTaskFactory;

        internal int subsequentPollCount;
        internal uint maxBatchSize;
        private readonly ILog logger;

        public SpanProcessor(Uri uri, BlockingCollection<Span> spanQueue, uint maxBatchSize)
        {
            if ( spanQueue == null) 
            {
                throw new ArgumentNullException("spanQueue");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.uri = uri;
            this.spanQueue = spanQueue;
            this.serializableSpans = new ConcurrentQueue<JsonSpan>();
            this.maxBatchSize = maxBatchSize;
            this.logger = LogProvider.GetCurrentClassLogger();
            spanProcessorTaskFactory = new SpanProcessorTaskFactory(null);
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
            var anyNewSpans = ProcessQueuedSpans();

            if (anyNewSpans) subsequentPollCount = 0;
            else if (serializableSpans.Count > 0) subsequentPollCount++;

            if (ShouldSendQueuedSpansOverWire())
            {
                SendSpansOverHttp();
            }
        }

        public virtual void SendSpansToZipkin(string spans)
        {
            if(spans == null) throw new ArgumentNullException("spans");
            using (var client = new WebClient())
            {
                try
                {
                    client.BaseAddress = uri.ToString();
                    client.UploadString(ZIPKIN_SPAN_POST_PATH, "POST", spans);
                }
                catch (WebException ex)
                {
                    //Very friendly HttpWebRequest Error message with good information.
                    LogHttpErrorMessage(ex);
                    throw;
                }
            }
        }

        private bool ShouldSendQueuedSpansOverWire()
        {
            return serializableSpans.Any() &&
                   (serializableSpans.Count() >= maxBatchSize
                   || spanProcessorTaskFactory.IsTaskCancelled()
                   || subsequentPollCount > MAX_NUMBER_OF_POLLS);
        }

        private bool ProcessQueuedSpans()
        {
            Span span;
            var anyNewSpansQueued = false;
            while (spanQueue.TryTake(out span))
            {
                serializableSpans.Enqueue(new JsonSpan(span));
                anyNewSpansQueued = true;
            }
            return anyNewSpansQueued;
        }

        private void SendSpansOverHttp()
        {
            var spansJsonRepresentation = GetSpansJSONRepresentation();
            SendSpansToZipkin(spansJsonRepresentation);
            subsequentPollCount = 0;
        }

        private string GetSpansJSONRepresentation()
        {
            JsonSpan span;
            var spanList = new List<JsonSpan>();
            //using Dequeue into a list so that the span is removed from the queue as we add it to list
            while (serializableSpans.TryDequeue(out span))
            {
                spanList.Add(span);
            }
            var spansJsonRepresentation = JsonConvert.SerializeObject(spanList);
            return spansJsonRepresentation;
        }

        private void LogHttpErrorMessage(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;
            if ((response == null)) return;
            var responseStream = response.GetResponseStream();
            var responseString = responseStream != null ? new StreamReader(responseStream).ReadToEnd() : string.Empty;
            logger.ErrorFormat(
                "Failed to send spans to Zipkin server (HTTP status code returned: {0}). Exception message: {1}, response from server: {2}",
                response.StatusCode, ex.Message, responseString);
        }
    }
}
