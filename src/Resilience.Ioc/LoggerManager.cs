using Serilog;

namespace Resilience.Ioc;

internal class LoggerManager
{
    public static ILogger Create()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }
}