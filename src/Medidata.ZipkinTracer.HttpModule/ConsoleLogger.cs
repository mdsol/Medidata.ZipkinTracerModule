using System.Diagnostics;
using zipkin4net;

namespace Medidata.ZipkinTracer.HttpModule
{
    public class ConsoleLogger : ILogger
    {
        public void LogInformation(string message)
        {
            Debug.WriteLine("Zipkin:Info:" + message);
        }

        public void LogWarning(string message)
        {
            Debug.WriteLine("Zipkin:Warning:" + message);
        }

        public void LogError(string message)
        {
            Debug.WriteLine("Zipkin:Error:" + message);
        }
    }
}