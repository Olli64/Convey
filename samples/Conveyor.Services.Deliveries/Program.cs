using System.Threading.Tasks;
using Convey;
using Convey.CQRS.Events;
using Convey.Discovery.Consul;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.AppMetrics;
using Convey.Monitoring.ApplicationInsights;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Conveyor.Services.Deliveries.Events.External;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Conveyor.Services.Deliveries
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
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddEventHandlers()
                        .AddRedis()
                        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                        .AddMetrics()
                        .AddMonitoring()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .Use(async (ctx, next) =>
                        {
                            ctx.Request.EnableBuffering();
                            await next();
                        })
                        //.UseRequestResponseLogging()
                        .UseConvey()
                        .UseErrorHandler()
                        .UseEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.Response.WriteAsync("Deliveries Service"))
                            .Get("ping", ctx => ctx.Response.WriteAsync("pong")))
                        .UseJaeger()
                        .UseMetrics()
                        .UseRabbitMq()
                        .SubscribeEvent<OrderCreated>())
                    .UseLogging();
            });
    }
}
