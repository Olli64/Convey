using System;

namespace Convey.Monitoring.ApplicationInsights
{
    internal sealed class InstrumentationKeyNotFoundException : Exception
    {

        public InstrumentationKeyNotFoundException(string message) : base(message)
        {
        }
    }
}
