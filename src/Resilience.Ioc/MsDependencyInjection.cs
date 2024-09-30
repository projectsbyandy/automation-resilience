using Microsoft.Extensions.DependencyInjection;
using Resilience.Retry;

namespace Resilience.Ioc;

/// <summary> Resilience Retry extension for Service Collection </summary>
public static class MsDependencyInjection
{
    /// <summary>
    /// Registers the IResilienceRetry into the MS Service Collection
    /// <param name="serviceCollection">Extension for Service Collection</param>
    /// <param name="addLoggerSupport">Registers an instance of Serilog ILogger for retry logging. Default: true</param>
    /// <param name="serviceLifetime">The ServiceLifeTime strategy to use during registration e.g. scoped</param>
    /// </summary>
    public static IServiceCollection AddResilienceSupport(this IServiceCollection serviceCollection,
        ServiceLifetime serviceLifetime, bool addLoggerSupport = true)
    {
        if (addLoggerSupport)
            serviceCollection.AddSingleton(LoggerManager.Create());
        
        serviceCollection.AddDynamic<IResilienceRetry, ResilienceRetry>(serviceLifetime);

        return serviceCollection;
    }
    
    private static void AddDynamic<TInterface, TClass>(this IServiceCollection services, ServiceLifetime lifetime) 
        where TClass : class, TInterface
        where TInterface : class
    {
        services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TClass), lifetime));
    }
}