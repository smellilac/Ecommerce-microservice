namespace Ecommerce.ProductService.DI;

using Serilog;

public class SerilogProductLogging
{
    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() 
            .MinimumLevel.Information()  
            .CreateLogger();
    }
}
