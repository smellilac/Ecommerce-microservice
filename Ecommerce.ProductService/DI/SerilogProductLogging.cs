namespace Ecommerce.ProductService.DI;

using Serilog;

public class SerilogProductLogging
{
    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Логирование в консоль
            .MinimumLevel.Information()  // Минимальный уровень логирования (например, только от информации и выше)
            .CreateLogger();
    }
}
