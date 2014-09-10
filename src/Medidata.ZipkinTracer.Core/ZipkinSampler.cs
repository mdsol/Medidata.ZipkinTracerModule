using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinSampler
    {
        private static Random random = new Random();
 
        private readonly List<string> dontSampleList;
        private readonly float sampleRate;
      
        public ZipkinSampler(List<string> dontSampleList, float sampleRate)
        {
            this.dontSampleList = dontSampleList;
            this.sampleRate = sampleRate;
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

        internal bool ShouldBeSampled(string path)
        {
            if ( ! IsInDontSampleList(path))
            {
                if ( random.NextDouble() <= sampleRate )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
