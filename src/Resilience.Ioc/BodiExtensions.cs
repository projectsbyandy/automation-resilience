using BoDi;
using Resilience.Retry;

namespace Resilience.Ioc;

public static class BodiExtensions
{
    public static ObjectContainer AddResilienceSupport(this ObjectContainer objectContainer, bool addLoggerSupport)
    {
        if (addLoggerSupport)
            objectContainer.RegisterInstanceAs(LoggerManager.Create());
            
        objectContainer.RegisterTypeAs<ResilienceRetry, ResilienceRetry>();
        
        return objectContainer;
    }
}