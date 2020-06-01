using Convey.Types;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Convey.Monitoring.ApplicationInsights.Initializers
{
    public class DefaultInitializer : ITelemetryInitializer
    {
        private readonly AppOptions _options;

        public DefaultInitializer(AppOptions options)
        {
            _options = options;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = _options.Name;
            }

            telemetry.Context.Component.Version = _options.Version;
        }
    }
}
