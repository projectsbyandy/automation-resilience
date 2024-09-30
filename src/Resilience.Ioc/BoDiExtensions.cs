using BoDi;
using Resilience.Retry;

namespace Resilience.Ioc;

/// <summary> Resilience Retry extension for BoDi Container </summary>
public static class BoDiExtensions
{
    /// <summary>
    /// Registers the IResilienceRetry into the BoDi Container
    /// <param name="objectContainer">Extension method on IObjectContainer</param>
    /// <param name="addLoggerSupport">Registers an instance of Serilog ILogger for retry logging. Default: true</param>
    /// </summary>
    public static IObjectContainer AddResilienceSupport(this IObjectContainer objectContainer, bool addLoggerSupport = true)
    {
        if (addLoggerSupport)
            objectContainer.RegisterInstanceAs(LoggerManager.Create());
            
        objectContainer.RegisterTypeAs<ResilienceRetry, IResilienceRetry>();
        
        return objectContainer;
    }
}