using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinFilter
    {
        private static Random random = new Random();
 
        private readonly List<string> filterList;
        private readonly float sampleRate;
      
        public ZipkinFilter(List<string> filterList, float sampleRate)
        {
            this.filterList = filterList;
            this.sampleRate = sampleRate;
        }

        internal bool IsInNonSampleList(string path)
        {
            if (path != null)
            {
                if (filterList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool ShouldBeSampled(string path)
        {
            if ( ! IsInNonSampleList(path))
            {
                var randomNumber = random.Next(10);
                
                if ( (float) (randomNumber * 0.1) <= sampleRate )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
