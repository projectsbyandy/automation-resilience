using FluentAssertions;
using Xunit;

namespace Resilience.Ioc.Tests;

public class LoggerManagerTests
{
    [Fact]
    public void Verify_LoggerManager_Create_Returns_Logger()
    {
        // Assemble / Act
        var logger = LoggerManager.Create();
        
        // Assert
        logger.Should().NotBeNull();
    }
}