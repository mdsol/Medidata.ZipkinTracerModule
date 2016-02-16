using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinConfig : IZipkinConfig
    {
        private double _sampleRage;

        public Uri ZipkinBaseUri { get; set; }
        public string ServiceName { get; set; }
        public uint SpanProcessorBatchSize { get; set; }
        public IList<string> DontSampleList { get; set; } = new List<string>();
        public double SampleRate
        {
            get { return _sampleRage; }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentException($"{nameof(SampleRate)} must range from 0 to 1.");
                _sampleRage = value;
            }
        }
        public string Domain { get; set; }
        public IList<string> NotToBeDisplayedDomainList { get; set; } = new List<string>();

        public bool ShouldBeSampled(IOwinContext context, string sampled)
        {
            if (context == null)
            {
                return false;
            }

            bool result;
            if (!string.IsNullOrWhiteSpace(sampled) && Boolean.TryParse(sampled, out result))
            {
                return result;
            }

            if (!IsInDontSampleList(context.Request.Path.ToString()))
            {
                var random = new Random();
                if (random.NextDouble() <= SampleRate)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsInDontSampleList(string path)
        {
            if (path != null)
            {
                if (DontSampleList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}