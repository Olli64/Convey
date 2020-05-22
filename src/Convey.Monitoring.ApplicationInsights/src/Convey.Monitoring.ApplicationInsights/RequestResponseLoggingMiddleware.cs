using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Convey.Monitoring.ApplicationInsights
{
    public class ApplicationInsightsLoggingMiddleware : IMiddleware
    {
        private readonly List<string> _pathsToIgnore;

        public ApplicationInsightsLoggingMiddleware(IConfiguration configuration)
        {
            _pathsToIgnore = configuration.GetSection("applicationInsights:ignorePaths").Get<List<string>>();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Inbound (before the controller)
            var request = context?.Request;
            if (request == null || !IsOkToSend(request.Path.Value))
            {
                await next(context);
                return;
            }

            request.EnableBuffering();  // Allows us to reuse the existing Request.Body

            // Swap the original Response.Body stream with one we can read / seek
            var originalResponseBody = context.Response.Body;
            await using var replacementResponseBody = new MemoryStream();
            context.Response.Body = replacementResponseBody;

            await next(context); // Continue processing (additional middleware, controller, etc.)

            // Outbound (after the controller)
            replacementResponseBody.Position = 0;

            // Copy the response body to the original stream
            await replacementResponseBody.CopyToAsync(originalResponseBody).ConfigureAwait(false);
            context.Response.Body = originalResponseBody;

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry == null)
            {
                return;
            }

            if (request.Body.CanRead)
            {
                var requestBodyString = await ReadBodyStream(request.Body).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(requestBodyString))
                {
                    requestTelemetry.Properties.Add("RequestBody", requestBodyString);
                }
            }

            if (request.QueryString.HasValue)
            {
                var querystring = request.QueryString.ToString();
                if (!string.IsNullOrEmpty(querystring))
                {
                    requestTelemetry.Properties.Add("querystring", querystring);
                }
            }

            if (replacementResponseBody.CanRead)
            {
                var responseBodyString = await ReadBodyStream(replacementResponseBody).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(responseBodyString))
                {
                    requestTelemetry.Properties.Add("ResponseBody", responseBodyString);
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

            using var reader = new StreamReader(body, leaveOpen: true);
            var bodyString = await reader.ReadToEndAsync().ConfigureAwait(false);
            body.Position = 0;

            return bodyString;
        }

        public bool IsOkToSend(string url)
        {
            if (url is null) return false;

            if (_pathsToIgnore is null || !_pathsToIgnore.Any()) return true;

            return !_pathsToIgnore.Any(url.Contains);
        }
    }
}
