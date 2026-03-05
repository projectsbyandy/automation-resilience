using FluentAssertions;
using Xunit;

namespace Resilience.Retry.Tests;

public class RetryOptionsTests
{
    private readonly RetryOptions _sut = new();

    [Fact]
    public void Verify_Retries_Options_Default_Value()
    {
        // Arrange / Act / Assert
        _sut.Delay.Should().Be(TimeSpan.FromSeconds(1));
        _sut.Retries.Should().Be(5);
        _sut.LogRetries.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(-3)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Verify_Retries_Less_Or_Equal_To_Zero_Throws_Argument_Exception(int retries)
    {
        // Arrange
        _sut.Retries = retries;
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Retries must be greater than zero.");
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(99)]
    public void Verify_Retries_Greater_Than_Zero_Passes_Validation(int retries)
    {
        // Arrange
        _sut.Retries = retries;
        
        // Act / Assert
        _sut.Validate();
    }
    
    [Theory]
    [InlineData(-3)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Verify_Delay_Less_Or_Equal_To_Zero_Throws_Argument_Exception(int timespanSeconds)
    {
        // Arrange
        _sut.Delay = TimeSpan.FromSeconds(timespanSeconds);
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Delay must be greater than zero.");
    }
    
    public static TheoryData<TimeSpan> DelayCasesLessThan1Hour => new()
    {
        TimeSpan.FromSeconds(3599),
        TimeSpan.FromSeconds(3598),
        TimeSpan.FromMinutes(59),
        TimeSpan.FromMinutes(6)
    };
    
    [Theory]
    [MemberData(nameof(DelayCasesLessThan1Hour))]    
    public void Verify_Delay_Greater_Than_Zero_Passes_Validation(TimeSpan delay)
    {
        // Arrange
        _sut.Delay = delay;
        
        // Act / Assert
        _sut.Validate();
    }

    public static TheoryData<TimeSpan> DelayCases1HourOrGreater => new()
    {
        TimeSpan.FromSeconds(3600),
        TimeSpan.FromSeconds(3601),
        TimeSpan.FromMinutes(60),
        TimeSpan.FromMinutes(61),
        TimeSpan.FromHours(1)
    };

    [Theory]
    [MemberData(nameof(DelayCases1HourOrGreater))]
    public void Verify_Delay_1hour_Or_More_Cannot_Be_Configured(TimeSpan delay)
    {
        // Arrange
        _sut.Delay = delay;
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Delay must be less than 1 hour");
    }
}