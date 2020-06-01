using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System;

namespace Convey.Monitoring.ApplicationInsights
{
    public interface IRequestTelemetryHandler
    {
        RequestTelemetry Handle(RequestTelemetry request, HttpContext context);
        RequestTelemetry Invoke(RequestTelemetry requestTelemetry, HttpContext context);
    }
}
