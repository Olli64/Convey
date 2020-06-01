using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Convey.Monitoring.ApplicationInsights.Middlewares
{
    public class RequestResponseLoggingMiddleware : IMiddleware
    {
        private readonly IEnumerable<IRequestTelemetryHandler> _handlers;
        private readonly MonitoringOptions _options;

        public RequestResponseLoggingMiddleware(MonitoringOptions options, IEnumerable<IRequestTelemetryHandler> handlers)
        {
            _handlers = handlers;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context?.Request;

            if (request == null || !IsOkToSend(request.Path.Value))
            {
                await next(context);
                return;
            }

            request.EnableBuffering();

            var originalResponseBody = context.Response.Body;
            await using var replacementResponseBody = new MemoryStream();

            context.Response.Body = replacementResponseBody;

            // Stream is open here still

            await next(context);

            // But here the stream is closed.

            replacementResponseBody.Position = 0; // <-- Throws an exception because stream is closed.

            await replacementResponseBody.CopyToAsync(originalResponseBody).ConfigureAwait(false);
            context.Response.Body = originalResponseBody;

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry == null)
            {
                return;
            }

            foreach (var handler in _handlers)
            {
                try
                {
                    requestTelemetry = handler.Invoke(requestTelemetry, context);
                }
                catch { /*ignored*/ }
            }

            if (request.Body.CanRead)
            {
                var requestBodyString = await ReadBodyStream(request.Body).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(requestBodyString))
                {
                    requestTelemetry.Properties.Add(_options.RequestResponseOptions?.RequestBodyPropertyName ?? "RequestBody", requestBodyString);
                }
            }

            if (request.QueryString.HasValue)
            {
                var querystring = request.QueryString.ToString();
                if (!string.IsNullOrEmpty(querystring))
                {
                    requestTelemetry.Properties.Add(_options.RequestResponseOptions?.QueryStringPropertyName ?? "QueryString", querystring);
                }
            }

            if (replacementResponseBody.CanRead)
            {
                var responseBodyString = await ReadBodyStream(replacementResponseBody).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(responseBodyString))
                {
                    requestTelemetry.Properties.Add(_options.RequestResponseOptions?.ResponseBodyPropertyName ?? "ResponseBody", responseBodyString);
                }
            }
        }

        private static async Task<string> ReadBodyStream(Stream body)
        {
            if (body.Length == 0)
            {
                return null;
            }

            body.Position = 0;

            var reader = new StreamReader(body);
            var bodyString = await reader.ReadToEndAsync().ConfigureAwait(false);
            body.Position = 0;

            return bodyString;
        }

        public bool IsOkToSend(string url)
        {
            if (url is null) return false;

            if (_options.IgnorePaths is null || !_options.IgnorePaths.Any()) return true;

            return !_options.IgnorePaths.Any(url.Contains);
        }
    }
}
