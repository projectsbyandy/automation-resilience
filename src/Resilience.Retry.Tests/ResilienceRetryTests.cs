using FluentAssertions;
using Moq;
using Serilog;

namespace Resilience.Retry.Tests
{
    internal class SynchronizationTests
    {
        private IResilienceRetry _sut;
        private Mock<ILogger> _loggerMock;
        private int _retryAttempts;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _sut = new ResilienceRetry(_loggerMock.Object);
            _retryAttempts = 0;
        }

        #region Tests for Void Retry

        [Test]
        public void Retry_Void_Does_Not_Throw_RetryException_On_Success()
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail" };

            // Act / Assert
            Assert.DoesNotThrow(() =>
            {
                _sut.Perform(() =>
                    VerifyTestResultList(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Never);
        }

        [TestCase(1, 2)]
        [TestCase(2, 3)]
        [TestCase(5, 6)]
        public void Retry_Void_Is_Successful_After_Retrying(int retriesTillSuccess, int retries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act / Assert
            Assert.DoesNotThrow(() =>
            {
                _sut.Perform(() =>
                        VerifyTestResultList(testResults, _retryAttempts, retriesTillSuccess),
                    TimeSpan.FromMilliseconds(1),
                    retries);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }
        
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-3)]
        public void Retry_throws_ArgumentException_when_retries_less_than_or_equal_to_zero(int retries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _sut.Perform(() =>
                        VerifyTestResultList(testResults, _retryAttempts, retries),
                    TimeSpan.FromMilliseconds(1),
                    retries);
            });
            
            // Assert
            exception.Message.Should().Be("Retry count should be greater than zero");
        }

        [Test]
        public void Retry_Void_Throws_Retry_Exception_After_Retry_Exhausted()
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act / Assert
            var exception = Assert.Throws<RetryException>(() =>
            {
                _sut.Perform(() =>
                        VerifyTestResultList(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            exception?.Message.Should().Be("Located 'retry' in list Retrying");
        }

        [TestCase(10, 10)]
        [TestCase(5, 5)]
        [TestCase(2, 2)]
        public void Retry_Void_Number_Of_Retries_Are_Logged(int retriesToThrow, int expectedNumberOfLogEntries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act
            Assert.Throws<RetryException>(() =>
            {
                _sut.Perform(() =>
                        VerifyTestResultList(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    retriesToThrow);
            });

            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(expectedNumberOfLogEntries));
        }

        [Test]
        public void Retry_Void_Does_Not_Retry_If_Non_RetryException_Thrown()
        {
            // Arrange
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _sut.Perform(ProcessThatDoesNotThrowRetry,
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            // Act / Assert
            exception?.Message.Should().Be("This Exception does not trigger a retry");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Never);
        }

        #endregion

        #region Tests for Void Retry Async

        [Test]
        public void Retry_Void_Async_Does_Not_Throw_RetryException_On_Success()
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail" };

            // Act / Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                await _sut.PerformAsync(async () =>
                    await VerifyTestResultListAsync(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Never);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-3)]
        public void RetryAsync_throws_ArgumentException_when_retries_less_than_or_equal_to_zero(int retries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _sut.PerformAsync(async () =>
                        await VerifyTestResultListAsync(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    retries);
            });
            
            // Assert
            exception.Message.Should().Be("Retry count should be greater than zero");
        }
        
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        [TestCase(5, 6)]
        public void Retry_Void_Async_Is_Successful_After_Retrying(int retriesTillSuccess, int retries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act / Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                await _sut.PerformAsync(async () =>
                        await VerifyTestResultListAsync(testResults, _retryAttempts, retriesTillSuccess),
                    TimeSpan.FromMilliseconds(1),
                    retries);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }

        [Test]
        public void Retry_Void_Async_Throws_Retry_Exception_After_Retry_Exhausted()
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act / Assert
            var exception = Assert.ThrowsAsync<RetryException>(async () =>
            {
                await _sut.PerformAsync(async () =>
                        await VerifyTestResultListAsync(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            exception?.Message.Should().Be("Located 'retry' in list");
        }

        [TestCase(10, 10)]
        [TestCase(5, 5)]
        [TestCase(2, 2)]
        public void Retry_Void_Async_Number_Of_Retries_Are_Logged(int retriesToThrow, int expectedNumberOfLogEntries)
        {
            // Arrange
            var testResults = new List<string>() { "pass", "fail", "retry" };

            // Act
            Assert.ThrowsAsync<RetryException>(async () =>
            {
                await _sut.PerformAsync(async () =>
                        await VerifyTestResultListAsync(testResults, _retryAttempts),
                    TimeSpan.FromMilliseconds(1),
                    retriesToThrow);
            });

            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(expectedNumberOfLogEntries));
        }

        [Test]
        public void Retry_Void_Async_Does_Not_Retry_If_Non_RetryException_Thrown()
        {
            // Arrange
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _sut.PerformAsync(async() => await ProcessThatDoesNotThrowRetryAsync(),
                    TimeSpan.FromMilliseconds(1),
                    3);
            });

            // Act / Assert
            exception?.Message.Should().Be("This Exception does not trigger a retry");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Never);
        }

        #endregion

        #region Tests for Return Retry

        [TestCase(6,10)]
        [TestCase(2,5)]
        [TestCase(1,3)]
        public void Retry_Return_Is_Successful_After_Retrying(int retriesTillSuccess, int maxRetriedBeforeFailure)
        {
            // Arrange / Act
            var ingested = _sut.PerformWithReturn(
                () => ReturnOutcomeWithRetryException(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetriedBeforeFailure);

            // Assert
            ingested.Should().BeTrue("Ingestion is successful");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }

        [TestCase(4,3)]
        [TestCase(6,3)]
        [TestCase(2,1)]
        public void Retry_Return_RetryException_Raised_On_Failure(int retriesTillSuccess, int maxRetriedBeforeFailure)
        {
            // Arrange
            var exception = Assert.Throws<RetryException>(() => _sut.PerformWithReturn(
                () => ReturnOutcomeWithRetryException(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetriedBeforeFailure));

            // Act / Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(maxRetriedBeforeFailure));
            
            exception?.Message.Should().Be("Problem with Ingestion Retrying");
        }

        #endregion

        #region Tests for Return Async Retry

        [TestCase(2, 3)]
        [TestCase(1, 2)]
        [TestCase(3, 4)]
        public async Task Retry_ReturnAsync_Successful(int retriesTillSuccess, int maxRetriedBeforeFailure)
        {
            // Arrange / Act
            var ingested = await _sut.PerformWithReturnAsync(
                async () => await ReturnOutcomeWithRetryExceptionAsync(true, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetriedBeforeFailure);

            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));

            ingested.Should().BeTrue("Ingestion Async is successful");
        }

        [TestCase(3,2)]
        [TestCase(6,4)]
        [TestCase(4,3)]
        public void Retry_ReturnAsync_Test_Failed(int retriesTillSuccess, int maxRetriedBeforeFailure)
        {
            // Arrange
            _retryAttempts = 0;
            
            // Act / Assert
            var exception = Assert.ThrowsAsync<RetryException>(async () => await _sut.PerformWithReturnAsync(
                async () => await ReturnOutcomeWithRetryExceptionAsync(false, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetriedBeforeFailure));

            exception?.Message.Should().Be("Problem with Ingestion Async Retrying");
        }

        #endregion

        #region Tests Retry Until True
        
        [TestCase(3,5)]
        [TestCase(2,4)]
        [TestCase(1,1)]
        public void Verify_Successful_Retry_Until_True(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            Assert.DoesNotThrow(()=> _sut.UntilTrue("Waiting for True", 
                () => ReturnOutcome(true, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for True", It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }
        
        [TestCase(2,1)]
        [TestCase(3,2)]
        [TestCase(10,4)]
        public void Verify_Exception_Thrown_on_Unsuccessful_Retry_Until_True(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            var exception = Assert.Throws<RetryException>(()=> _sut.UntilTrue("Waiting for True", 
                () => ReturnOutcome(true, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for True", It.IsAny<TimeSpan>()),
                Times.Exactly(maxRetries));
            
            exception.Message.Should().Be("Waiting for True");
        }
        
        #endregion
        
        #region Tests Retry Until False
        
        [TestCase(3,5)]
        [TestCase(2,4)]
        [TestCase(1,1)]
        public void Verify_Successful_Retry_Until_False(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            Assert.DoesNotThrow(()=> _sut.UntilFalse("Waiting for False", 
                () => ReturnOutcome(false, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for False", It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }
        
        [TestCase(2,1)]
        [TestCase(3,2)]
        [TestCase(10,4)]
        public void Verify_Exception_Thrown_on_Unsuccessful_Retry_Until_False(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            var exception = Assert.Throws<RetryException>(()=> _sut.UntilFalse("Waiting for False", 
                () => ReturnOutcome(false, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for False", It.IsAny<TimeSpan>()),
                Times.Exactly(maxRetries));
            
            exception.Message.Should().Be("Waiting for False");
        }
        
        #endregion
        
        #region Tests Retry Until True Async

        [TestCase(3,5)]
        [TestCase(2,4)]
        [TestCase(1,1)]
        public void Verify_Successful_Retry_Until_True_Async(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            Assert.DoesNotThrowAsync(()=> _sut.UntilTrueAsync("Waiting for True Async", 
                () => ReturnOutcomeAsync(true, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for True Async", It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }
        
        [TestCase(2,1)]
        [TestCase(3,2)]
        [TestCase(10,4)]
        public void Verify_Exception_Thrown_on_Unsuccessful_Retry_Until_True_Async(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            var exception = Assert.ThrowsAsync<RetryException>(()=> _sut.UntilTrueAsync("Waiting for True Async", 
                () => ReturnOutcomeAsync(true, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for True Async", It.IsAny<TimeSpan>()),
                Times.Exactly(maxRetries));
            
            exception.Message.Should().Be("Waiting for True Async");
        }
        
        #endregion
        
        #region Tests Retry Until False Async

        [TestCase(3,5)]
        [TestCase(2,4)]
        [TestCase(1,1)]
        public void Verify_Successful_Retry_Until_False_Async(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            Assert.DoesNotThrowAsync(()=> _sut.UntilFalseAsync("Waiting for False Async", 
                () => ReturnOutcomeAsync(false, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for False Async", It.IsAny<TimeSpan>()),
                Times.Exactly(retriesTillSuccess));
        }
        
        [TestCase(2,1)]
        [TestCase(3,2)]
        [TestCase(10,4)]
        public void Verify_Exception_Thrown_on_Unsuccessful_Retry_Until_False_Async(int retriesTillSuccess, int maxRetries)
        {
            // Arrange / Act
            var exception = Assert.ThrowsAsync<RetryException>(()=> _sut.UntilFalseAsync("Waiting for False Async", 
                () => ReturnOutcomeAsync(false, _retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(1), maxRetries));
            
            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    "Waiting for False Async", It.IsAny<TimeSpan>()),
                Times.Exactly(maxRetries));
            
            exception.Message.Should().Be("Waiting for False Async");
        }
        
        #endregion
        
        #region Helpers

        private void VerifyTestResultList(List<string> testResults, int retryAttempts, int retriesTillSuccess = 50)
        {
            if (retryAttempts == retriesTillSuccess)
                return;
                
            if (testResults.Exists(item => item.Equals("retry")))
            {
                _retryAttempts++;
                throw new RetryException("Located 'retry' in list Retrying");
            }
        }

        private async Task VerifyTestResultListAsync(List<string> testResults, int retryAttempts, int retriesTillSuccess = 50)
        {
            if (retryAttempts == retriesTillSuccess)
            {
                await Task.FromResult(true);
                return;
            }

            if (testResults.Exists(item => item.Equals("retry")))
            {
                _retryAttempts++;
                await Task.FromResult(false);
                
                throw new RetryException("Located 'retry' in list");
            }
        }

        private static void ProcessThatDoesNotThrowRetry() => throw new ArgumentException("This Exception does not trigger a retry");

        private static async Task ProcessThatDoesNotThrowRetryAsync()
        {
            await Task.FromResult(true);
            throw new ArgumentException("This Exception does not trigger a retry");
        }

        private bool ReturnOutcomeWithRetryException(int retryAttempt, int numberOfRetriesTillSuccess)
        {
            return ReturnOutcome(true, retryAttempt, numberOfRetriesTillSuccess) is false
                ? throw new RetryException("Problem with Ingestion Retrying")
                : true;
        }

        private async Task<bool> ReturnOutcomeWithRetryExceptionAsync(bool expectedOutcome, int retryAttempt, int numberOfRetriesTillSuccess)
        {
            return await ReturnOutcomeAsync(true, retryAttempt, numberOfRetriesTillSuccess) is false
                ? throw new RetryException("Problem with Ingestion Async Retrying")
                : await Task.FromResult(expectedOutcome);
        }

        private bool ReturnOutcome(bool expectedOutcome, int retryAttempt, int numberOfRetriesTillSuccess)
        {
            if (retryAttempt == numberOfRetriesTillSuccess)
                return expectedOutcome;
            
            _retryAttempts++;
            return !expectedOutcome;
        }
        
        private async Task<bool> ReturnOutcomeAsync(bool expectedOutcome, int retryAttempt, int numberOfRetriesTillSuccess)
        {
            return await Task.FromResult(ReturnOutcome(expectedOutcome, retryAttempt, numberOfRetriesTillSuccess));
        }

        #endregion
    }
}