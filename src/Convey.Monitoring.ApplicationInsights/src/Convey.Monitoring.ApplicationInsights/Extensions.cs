using Convey.Monitoring.ApplicationInsights.Initializers;
using Convey.Monitoring.ApplicationInsights.Middlewares;
using Convey.Monitoring.ApplicationInsights.Processors;
using Convey.Types;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;

namespace Convey.Monitoring.ApplicationInsights
{
    public static class Extensions
    {
        private static MonitoringOptions _options;
        private static AppOptions _appOptions;
        private const string MonitoringSectionName = "monitoring";

        public static IConveyBuilder AddMonitoring(this IConveyBuilder builder,
            Action<ApplicationInsightsServiceOptions> options = null,
            string monitoringSectionName = MonitoringSectionName,
            string instrumentationkey = null)
        {
            using var sp = builder.Services.BuildServiceProvider();
            var config = sp.GetRequiredService<IConfiguration>();
            _appOptions = sp.GetRequiredService<AppOptions>();

            _options = config.GetOptions<MonitoringOptions>(monitoringSectionName);

            builder.Services.AddSingleton(_options);

            if (instrumentationkey is null)
            { 
                instrumentationkey = config.GetValue<string>("applicationInsights:instrumentationKey");
            }

            if (instrumentationkey is null)
            {
                throw new InstrumentationKeyNotFoundException("Could not find instrumentationkey. Cannot initialize ApplicationInsights without it. Please provide it in AddMonitoring() parameters or use appsettings.");
            }

            _options.InstrumentationKey = instrumentationkey;

            builder.Services.AddSingleton<ITelemetryInitializer, DefaultInitializer>();
            builder.Services.AddTransient<RequestResponseLoggingMiddleware>();

            builder.Services.AddApplicationInsightsTelemetry(options ?? AppInsOptions);

            if (!(_options.KubernetesOptions is null) && _options.KubernetesOptions.Enabled)
            {
                builder.Services.AddApplicationInsightsKubernetesEnricher(KubeOptions);
            }

            Activity.DefaultIdFormat = _options.UseW3CIdFormat ? ActivityIdFormat.W3C : ActivityIdFormat.Hierarchical;

            if (!(_options.IgnorePaths is null) && _options.IgnorePaths.Any())
            {
                builder.Services.AddApplicationInsightsTelemetryProcessor<IgnorePathsFilter>();
            }

            return builder;
        }

        private static void AppInsOptions(ApplicationInsightsServiceOptions obj)
        {
            obj.InstrumentationKey = _options.InstrumentationKey;
            obj.RequestCollectionOptions.TrackExceptions = _options.TrackExceptions;
            obj.ApplicationVersion = _appOptions.Version;
            obj.EnableAdaptiveSampling = _options.EnableAdaptiveSampling;
            obj.EnableQuickPulseMetricStream = _options.EnableQuickPulseMetricStream;
            obj.RequestCollectionOptions.InjectResponseHeaders = _options.InjectResponseHeaders;
        }

        private static void KubeOptions(AppInsightsForKubernetesOptions obj)
        {
            obj.InitializationTimeout = _options.KubernetesOptions.InitializeTimeout;
        }

        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
