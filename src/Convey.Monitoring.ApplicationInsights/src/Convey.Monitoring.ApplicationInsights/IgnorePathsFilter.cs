using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace Convey.Monitoring.ApplicationInsights
{
    public class IgnorePathsFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; }
        private readonly List<string> _pathsToIgnore;

        public IgnorePathsFilter(ITelemetryProcessor next, IConfiguration configuration)
        {
            Next = next;
            _pathsToIgnore = configuration.GetSection("applicationInsights:ignorePaths").Get<List<string>>();
        }

        public void Process(ITelemetry item)
        {
            if (!IsOkToSend(item)) return;

            Next.Process(item);
        }

        public bool IsOkToSend(ITelemetry item)
        {
            if (!(item is RequestTelemetry request)) return true;

            if (_pathsToIgnore is null || !_pathsToIgnore.Any()) return true;

            return !_pathsToIgnore.Any(p => request.Url.AbsoluteUri.Contains(p));
        }
    }
}
