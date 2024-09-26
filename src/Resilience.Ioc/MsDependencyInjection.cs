using Microsoft.Extensions.DependencyInjection;
using Resilience.Retry;

namespace Resilience.Ioc;

public static class MsDependencyInjection
{
    public static IServiceCollection AddResilienceSupport(this IServiceCollection serviceCollection,
        bool addLoggerSupport = true)
    {
        if (addLoggerSupport)
            serviceCollection.AddSingleton(LoggerManager.Create());
        
        serviceCollection.AddSingleton<IResilienceRetry, ResilienceRetry>();

        return serviceCollection;
    }

}