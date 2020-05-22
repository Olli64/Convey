using System.Collections.Generic;

namespace Convey.Monitoring.ApplicationInsights
{
    public class ApplicationInsightsOptions
    {
        public bool TrackExceptions { get; set; } = true;
        public string ApplicationVersion { get; set; } = "1.0.0";
        public bool EnableAdaptiveSampling { get; set; } = false;
        public bool EnableQuickPulseMetricStream { get; set; } = true;
        public bool InjectResponseHeaders { get; set; } = true;
        public bool UseW3CIdFormat { get; set; } = true;
        public List<string> IgnorePaths { get; set; }
        public KubernetesOptions KubernetesOptions { get; set; }
    }
}
