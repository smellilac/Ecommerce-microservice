using Serilog;

namespace Ecommerce.OrderService.DI;

public class SerilogOrderLogging
{
    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Information()
            .CreateLogger();
    }
}
