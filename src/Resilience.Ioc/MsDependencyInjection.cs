using Microsoft.Extensions.DependencyInjection;
using Resilience.Retry;

namespace Resilience.Ioc;

public static class MsDependencyInjection
{
    public static ServiceCollection AddResilienceSupport(this ServiceCollection serviceCollection,
        bool addLoggerSupport)
    {
        if (addLoggerSupport)
            serviceCollection.AddSingleton(LoggerManager.Create());
        
        serviceCollection.AddSingleton<IResilienceRetry, ResilienceRetry>();

        return serviceCollection;
    }

}