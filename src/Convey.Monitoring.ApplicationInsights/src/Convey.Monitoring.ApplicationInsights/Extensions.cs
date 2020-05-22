using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Monitoring.ApplicationInsights
{
    public static class Extensions
    {
        public static IConveyBuilder AddApplicationInsights(this IConveyBuilder builder, Action<ApplicationInsightsBuilder> aiAction = null)
        {
        }

        public static IApplicationInsightsBuilder AddTelemetryProcessor<T>(this IApplicationInsightsBuilder builder) where T : ITelemetryProcessor
        {
            builder.Builder.Services.AddApplicationInsightsTelemetryProcessor<T>();

            return builder;
        }

        public static IApplicationInsightsBuilder AddPathFilter(this IApplicationInsightsBuilder builder)
            => builder.AddTelemetryProcessor<IgnorePathsFilter>();

        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApplicationInsightsLoggingMiddleware>();
        }
    }
}
