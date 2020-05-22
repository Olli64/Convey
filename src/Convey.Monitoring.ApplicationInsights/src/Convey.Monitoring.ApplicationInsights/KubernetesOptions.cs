using System;

namespace Convey.Monitoring.ApplicationInsights
{
    public class KubernetesOptions
    {
        public bool Enabled { get; set; } = false;
        public TimeSpan InitializeTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
