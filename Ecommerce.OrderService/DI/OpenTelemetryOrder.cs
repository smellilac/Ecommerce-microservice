using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ecommerce.OrderService.DI;

public static class OpenTelemetryOrder
{
    public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingOptions =>
            {
                tracingOptions
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ProductService"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = "localhost";
                        jaegerOptions.AgentPort = 6831;
                    });
            })
            .WithMetrics(metricsOptions =>
            {
                metricsOptions
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });
    }
}
