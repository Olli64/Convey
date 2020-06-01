using System.Threading.Tasks;
using Convey;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.Metrics.AppMetrics;
using Convey.Monitoring.ApplicationInsights;
using Convey.Tracing.Jaeger;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Convey.WebApi.Security;
using Conveyor.Services.Pricing.DTO;
using Conveyor.Services.Pricing.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Open.Serialization.Json;

namespace Conveyor.Services.Pricing
{
    public class Program
    {
        public static Task Main(string[] args)
            => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => services
                        .AddConvey()
                        .AddErrorHandler<ExceptionToResponseMapper>()
                        .AddQueryHandlers()
                        .AddInMemoryQueryDispatcher()
                        .AddServices()
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddMonitoring()
                        .AddMetrics()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .Use(async (ctx, next) =>
                        {
                            ctx.Request.EnableBuffering();
                            await next();
                        })
                        .UseConvey()
                        .UseErrorHandler()
                        .UseJaeger()
                        .UseMetrics()
                        .UseCertificateAuthentication()
                        .UseAuthentication()
                        .UseAuthorization()
                        .UseRequestResponseLogging()
                        .UseEndpoints(endpoints => endpoints
                                .Get("", ctx => ctx.Response.WriteAsync("Pricing Service"))
                                .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
                        )
                        .UseDispatcherEndpoints(endpoints => endpoints
                            .Get<GetOrderPricing, PricingDto>("orders/{orderId}/pricing"))
                        )
                    .UseLogging();
            });
    }
}
