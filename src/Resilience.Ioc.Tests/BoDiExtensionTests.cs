using BoDi;
using FluentAssertions;
using Moq;
using Resilience.Retry;
using Serilog;
using Xunit;

namespace Resilience.Ioc.Tests;

public class BoDiExtensionTests : IDisposable
{
    private readonly IObjectContainer? _container = new ObjectContainer();
    private readonly Mock<ILogger>? _loggerMock = new();

    [Fact]
    public void Verify_Default_Resilience_Extension_Can_Be_Registered()
    {
        // Arrange / Act
        _container?.AddResilienceSupport();
        
        // Assert
        _container?.Resolve<IResilienceRetry>().Should().NotBeNull();
    }
    
    [Fact]
    public void Verify_Logging_Is_Added_With_Default_Resilience_Extension()
    {
        // Arrange / Act
        _container?.AddResilienceSupport();
        
        // Assert
        _container?.Resolve<ILogger>().Should().NotBeNull();
    }
    
    [Fact]
    public void Verify_Resolving_IResilienceRetry_With_No_Logger_Throws_Exception()
    {
        // Arrange / Act
        _container?.AddResilienceSupport(false);
        
        // Assert
        var exception = Assert.Throws<ObjectContainerException>(()=> _container?.Resolve<IResilienceRetry>());
        exception?.Message.Should().Be("Interface cannot be resolved: Serilog.ILogger (resolution path: Resilience.Retry.ResilienceRetry)");
    }
    
    [Fact]
    public void Verify_Resolving_IResilienceRetry_With_Manually_Created_Logger()
    {
        // Arrange
        _container?.RegisterInstanceAs(_loggerMock?.Object);
        
        // Act
        _container?.AddResilienceSupport(false);
        
        // Assert
        _container?.Resolve<IResilienceRetry>().Should().NotBeNull();
    }
    
    public void Dispose()
    {
        _container?.Dispose();
    }
}