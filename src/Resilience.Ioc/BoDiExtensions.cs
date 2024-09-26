using BoDi;
using Resilience.Retry;

namespace Resilience.Ioc;

public static class BoDiExtensions
{
    public static IObjectContainer AddResilienceSupport(this IObjectContainer objectContainer, bool addLoggerSupport = true)
    {
        if (addLoggerSupport)
            objectContainer.RegisterInstanceAs(LoggerManager.Create());
            
        objectContainer.RegisterTypeAs<ResilienceRetry, IResilienceRetry>();
        
        return objectContainer;
    }
}