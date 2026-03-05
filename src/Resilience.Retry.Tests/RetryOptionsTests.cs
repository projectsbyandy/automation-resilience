using FluentAssertions;

namespace Resilience.Retry.Tests;

internal class RetryOptionsTests
{
    private RetryOptions _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new RetryOptions();
    }

    [Test]
    public void Verify_Retries_Options_Default_Value()
    {
        // Arrange / Act / Assert
        _sut.Delay.Should().Be(TimeSpan.FromSeconds(1));
        _sut.Retries.Should().Be(5);
        _sut.LogRetries.Should().BeFalse();
    }
    
    [TestCase(-3)]
    [TestCase(-1)]
    [TestCase(0)]
    public void Verify_Retries_Less_Or_Equal_To_Zero_Throws_Argument_Exception(int retries)
    {
        // Arrange
        _sut.Retries = retries;
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Retries must be greater than zero.");
    }
    
    [TestCase(1)]
    [TestCase(10)]
    [TestCase(99)]
    public void Verify_Retries_Greater_Than_Zero_Passes_Validation(int retries)
    {
        // Arrange
        _sut.Retries = retries;
        
        // Act / Assert
        Assert.DoesNotThrow(() => _sut.Validate());
    }
    
    [TestCase(-3)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(0)]
    public void Verify_Delay_Less_Or_Equal_To_Zero_Throws_Argument_Exception(int timespanSeconds)
    {
        // Arrange
        _sut.Delay = TimeSpan.FromSeconds(timespanSeconds);
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Delay must be greater than zero.");
    }
    
    public static IEnumerable<TestCaseData> DelayCasesLessThan1Hour()
    {
        yield return new TestCaseData(TimeSpan.FromSeconds(3599));
        yield return new TestCaseData(TimeSpan.FromSeconds(3598));
        yield return new TestCaseData(TimeSpan.FromMinutes(59));
        yield return new TestCaseData(TimeSpan.FromMinutes(6));
    }
    
    [TestCaseSource(nameof(DelayCasesLessThan1Hour))]
    public void Verify_Delay_Greater_Than_Zero_Passes_Validation(TimeSpan delay)
    {
        // Arrange
        _sut.Delay = delay;
        
        // Act / Assert
        Assert.DoesNotThrow(() => _sut.Validate());
    }

    public static IEnumerable<TestCaseData> DelayCases1HourOrGreater()
    {
        yield return new TestCaseData(TimeSpan.FromSeconds(3600));
        yield return new TestCaseData(TimeSpan.FromSeconds(3601));
        yield return new TestCaseData(TimeSpan.FromMinutes(60));
        yield return new TestCaseData(TimeSpan.FromMinutes(61));
        yield return new TestCaseData(TimeSpan.FromHours(1));
    }

    [TestCaseSource(nameof(DelayCases1HourOrGreater))]
    public void Verify_Delay_1hour_Or_More_Cannot_Be_Configured(TimeSpan delay)
    {
        // Arrange
        _sut.Delay = delay;
        
        // Act / Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Validate());
        
        exception.Message.Should().Be("Delay must be less than 1 hour");
    }
}