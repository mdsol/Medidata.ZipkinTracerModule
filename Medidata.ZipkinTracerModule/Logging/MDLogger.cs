using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Logging
{
    //This class is just a placeholder.  
    //Once the .NET logging module is completed, that will have the concrete class for logging
    //This class will allow the IMDLogger interface to get resolved in Unity since it is being injected into ZipkinTracerModule.
    public class MDLogger : IMDLogger
    {
        public string Component
        {
            get { throw new NotImplementedException(); }
        }

        public string Tenant
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ComponentVersion
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Machine
        {
            get { throw new NotImplementedException(); }
        }

        public string Context
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Guid TraceId
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SpanId
        {
            get { throw new NotImplementedException(); }
        }

        public Guid? ParentSpanId
        {
            get { throw new NotImplementedException(); }
        }

        public void Event(string message, HashSet<string> tags, object data)
        {
        }

        public void Event(string message, HashSet<string> tags, object data, Exception e)
        {
        }

        public string EventRow(string message, HashSet<string> tags, object data, Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
