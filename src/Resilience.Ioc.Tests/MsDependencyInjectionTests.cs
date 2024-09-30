using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Resilience.Retry;
using Serilog;

namespace Resilience.Ioc.Tests;

internal class MsDependencyInjectionTests
{
    
    private Mock<ILogger>? _loggerMock;
    private IServiceCollection? _serviceCollection;
    private IServiceProvider? _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
        _serviceCollection = new ServiceCollection();
    }

    [Test]
    public void Verify_Default_Resilience_Extension_Can_Be_Registered()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped);

        // Act
        _serviceProvider = _serviceCollection?.BuildServiceProvider();

        // Assert
        _serviceProvider?.GetService<IResilienceRetry>().Should().NotBeNull();
    }

    [Test]
    public void Verify_Resilience_Extension_Can_Be_Registered_As_Scoped()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped);

        // Act
        _serviceProvider = _serviceCollection?.BuildServiceProvider();
        
        // Assert
        using (var scope1 = _serviceProvider?.CreateScope())
        {
            var resilienceRetry1 = scope1?.ServiceProvider.GetService<IResilienceRetry>();
            using (var scope2 = _serviceProvider?.CreateScope())
            {
                var resilienceRetry2 = scope2?.ServiceProvider.GetService<IResilienceRetry>();

                // Assert
                resilienceRetry1.Should().NotBeNull();
                resilienceRetry2.Should().NotBeNull();
                resilienceRetry1.Should().NotBe(resilienceRetry2);
            }
        }
    }

    [Test]
    public void Verify_Resilience_Extension_Can_Be_Registered_As_Singleton()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped);

        // Act
        _serviceProvider = _serviceCollection?.BuildServiceProvider();

        // Assert
        var resilienceRetry1 = _serviceProvider?.GetService<IResilienceRetry>();
        var resilienceRetry2 = _serviceProvider?.GetService<IResilienceRetry>();
        
        resilienceRetry1.Should().NotBeNull();
        resilienceRetry2.Should().NotBeNull();
        
        resilienceRetry1.Should().Be(resilienceRetry2);
    }

    [Test]
    public void Verify_Logging_Is_Added_With_Default_Resilience_Extension()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped);
        
        // Act
        _serviceProvider = _serviceCollection?.BuildServiceProvider();
            
        // Assert
        _serviceProvider?.GetService<ILogger>().Should().NotBeNull();
    }
    
    [Test]
    public void Verify_Resolving_IResilienceRetry_With_No_Logger_Throws_Exception()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped, false);
        
        // Act
        _serviceProvider = _serviceCollection?.BuildServiceProvider();
        
        // Assert
        var exception = Assert.Throws<InvalidOperationException>(()=> _serviceProvider?.GetService<IResilienceRetry>());
        exception?.Message.Should().Be("Unable to resolve service for type 'Serilog.ILogger' while attempting to activate 'Resilience.Retry.ResilienceRetry'.");
    }
    
    [Test]
    public void Verify_Resolving_IResilienceRetry_With_Manually_Created_Logger()
    {
        // Arrange
        _serviceCollection?.AddResilienceSupport(ServiceLifetime.Scoped, false);
        _serviceCollection?.AddScoped(_ => _loggerMock!.Object);
        
        // Act
        var serviceProvider = _serviceCollection?.BuildServiceProvider();
        
        // Assert
        Assert.DoesNotThrow(()=> serviceProvider?.GetService<IResilienceRetry>());
    }
    
    [TearDown]
    public void TearDown()
    {
         _serviceProvider = null;
    }
}