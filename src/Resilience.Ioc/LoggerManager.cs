using System.Runtime.CompilerServices;
using Serilog;

[assembly:InternalsVisibleTo("Resilience.Ioc.Tests")]

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
