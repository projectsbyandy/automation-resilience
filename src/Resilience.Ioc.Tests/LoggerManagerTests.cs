using FluentAssertions;
using NUnit.Framework;

namespace Resilience.Ioc.Tests;

internal class LoggerManagerTests
{
    [Test]
    public void Verify_LoggerManager_Create_Returns_Logger()
    {
        // Assemble / Act
        var logger = LoggerManager.Create();
        
        // Assert
        logger.Should().NotBeNull();
    }
}