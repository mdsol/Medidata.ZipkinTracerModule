using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinSampler
    {
        private static Random random = new Random();
 
        internal readonly List<string> dontSampleList;
        internal readonly float sampleRate;
      
        public ZipkinSampler(string dontSampleListCsv, string configSampleRate)
        {
            var dontSampleList = new List<string>();
            if (!String.IsNullOrWhiteSpace(dontSampleListCsv))
            {
                dontSampleList.AddRange(dontSampleListCsv.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim().ToLowerInvariant()));
            }

            float zipkinSampleRate;
            float.TryParse(configSampleRate, out zipkinSampleRate);
            if ( zipkinSampleRate < 0 || zipkinSampleRate > 1)
            {
                throw new ArgumentException("zipkinConfig zipkinSampleRate is not between 0 and 1");
            }

            this.dontSampleList = dontSampleList;
            this.sampleRate = zipkinSampleRate;
        }

        internal bool IsInDontSampleList(string path)
        {
            if (path != null)
            {
                if (dontSampleList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ShouldBeSampled(IOwinContext context, string sampled)
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
                if (random.NextDouble() <= sampleRate)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
