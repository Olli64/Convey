using System;
using System.Collections.Generic;

namespace Convey.Monitoring.ApplicationInsights
{
    public class MonitoringOptions
    {
        public string InstrumentationKey { get; set; }
        public bool TrackExceptions { get; set; } = true;
        public bool EnableAdaptiveSampling { get; set; } = false;
        public bool EnableQuickPulseMetricStream { get; set; } = true;
        public bool InjectResponseHeaders { get; set; } = true;
        public bool UseW3CIdFormat { get; set; } = true;
        public List<string> IgnorePaths { get; set; }
        public RequestResponseOptions RequestResponseOptions { get; set; }
        public KubernetesOptions KubernetesOptions { get; set; }
    }

    public class KubernetesOptions
    {
        public bool Enabled { get; set; } = false;
        public TimeSpan InitializeTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }

    public class RequestResponseOptions
    {
        public string RequestBodyPropertyName { get; set; }
        public string ResponseBodyPropertyName { get; set; }
        public string QueryStringPropertyName { get; set; }
    }
}
