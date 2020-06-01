using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Linq;

namespace Convey.Monitoring.ApplicationInsights.Processors
{
    public class IgnorePathsFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; }
        private readonly MonitoringOptions _options;

        public IgnorePathsFilter(ITelemetryProcessor next, MonitoringOptions options)
        {
            Next = next;
            _options = options;
        }

        public void Process(ITelemetry item)
        {
            if (!IsOkToSend(item)) return;

            Next.Process(item);
        }

        public bool IsOkToSend(ITelemetry item)
        {
            if (!(item is RequestTelemetry request)) return true;

            if (_options.IgnorePaths is null || !_options.IgnorePaths.Any()) return true;

            return !_options.IgnorePaths.Any(p => request.Url.AbsoluteUri.Contains(p));
        }
    }
}
