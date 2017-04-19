using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinConfig : IZipkinConfig
    {
        private Random random = new Random();

        public Predicate<HttpRequestBase> Bypass { get; set; } = r => false;
        public Uri ZipkinBaseUri { get; set; }
        public Func<HttpRequestBase, Uri> Domain { get; set; }
        public uint SpanProcessorBatchSize { get; set; }
        public IList<string> ExcludedPathList { get; set; } = new List<string>();
        public double SampleRate { get; set; }
        public IList<string> NotToBeDisplayedDomainList { get; set; } = new List<string>();
        public bool Create128BitTraceId { get; set; }

        public void Validate()
        {
            if (ZipkinBaseUri == null)
            {
                throw new ArgumentNullException("ZipkinBaseUri");
            }

            if (Domain == null)
            {
                Domain = request => new Uri(request.Url.Host);
            }

            if (ExcludedPathList == null)
            {
                throw new ArgumentNullException("ExcludedPathList");
            }

            if (ExcludedPathList.Any(item => !item.StartsWith("/")))
            {
                throw new ArgumentException("Item of ExcludedPathList must start with '/'. e.g.) '/check_uri'");
            }

            if (SampleRate < 0 || SampleRate > 1)
            {
                throw new ArgumentException("SampleRate must range from 0 to 1.");
            }

            if (NotToBeDisplayedDomainList == null)
            {
                throw new ArgumentNullException("NotToBeDisplayedDomainList");
            }
        }

        /// <summary>
        /// Checks if sampled flag from headers has value if not decide if need to sample or not using sample rate
        /// </summary>
        /// <param name="sampledFlag"></param>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        public bool ShouldBeSampled(string sampledFlag, string requestPath)
        {
            bool result;
            if (TryParseSampledFlagToBool(sampledFlag, out result)) return result;

            if (IsInDontSampleList(requestPath)) return false;
            
            return random.NextDouble() <= SampleRate;
        }

        internal bool IsInDontSampleList(string path)
        {
            if (path != null)
            {
                if (ExcludedPathList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Try parse sampledFlag to bool
        /// </summary>
        /// <param name="sampledFlag"></param>
        /// <param name="booleanValue"></param>
        /// <returns>returns true if sampledFlag can be parsed to bool</returns>
        private bool TryParseSampledFlagToBool(string sampledFlag, out bool booleanValue)
        {
            booleanValue = false;

            if (string.IsNullOrWhiteSpace(sampledFlag)) return false;

            switch (sampledFlag)
            {
                case "0":
                    booleanValue = false;
                    return true;
                case "1":
                    booleanValue = true;
                    return true;
                default:
                    return bool.TryParse(sampledFlag, out booleanValue);
            }
        }
    }
}