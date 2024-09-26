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
                    TimeSpan.FromMilliseconds(100),
                    3);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
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
                    TimeSpan.FromMilliseconds(100),
                    retries);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(retriesTillSuccess));
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
                    TimeSpan.FromMilliseconds(10),
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
                    TimeSpan.FromMilliseconds(10),
                    retriesToThrow);
            });

            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(expectedNumberOfLogEntries));
        }

        [Test]
        public void Retry_Void_Does_Not_Retry_If_Non_RetryException_Thrown()
        {
            // Arrange
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _sut.Perform(ProcessThatDoesNotThrowRetry,
                    TimeSpan.FromMilliseconds(10),
                    3);
            });

            // Act / Assert
            exception?.Message.Should().Be("This Exception does not trigger a retry");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
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
                    TimeSpan.FromMilliseconds(100),
                    3);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);
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
                    TimeSpan.FromMilliseconds(100),
                    retries);
            });

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
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
                    TimeSpan.FromMilliseconds(10),
                    3);
            });

            exception?.Message.Should().Be("Located 'retry' in list Retrying");
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
                    TimeSpan.FromMilliseconds(10),
                    retriesToThrow);
            });

            // Assert
            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(expectedNumberOfLogEntries));
        }

        [Test]
        public void Retry_Void_Async_Does_Not_Retry_If_Non_RetryException_Thrown()
        {
            // Arrange
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _sut.PerformAsync(async() => await ProcessThatDoesNotThrowRetryAsync(),
                    TimeSpan.FromMilliseconds(10),
                    3);
            });

            // Act / Assert
            exception?.Message.Should().Be("This Exception does not trigger a retry");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);
        }

        #endregion

        #region Tests for Return Retry

        [Test]
        public void Retry_Return_Is_Successful_After_Retrying()
        {
            // Arrange
            const int retriesTillSuccess = 2;
            const int maxRetriedBeforeFailure = 3;

            // Act
            var ingested = _sut.PerformReturn(
                () => WaitForThirdPartyIngestion(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(10), maxRetriedBeforeFailure);

            // Assert
            ingested.Should().BeTrue("Ingestion is successful");

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(retriesTillSuccess));
        }

        [Test]
        public void Retry_Return_RetryException_Raised_On_Failure()
        {
            // Arrange
            const int retriesTillSuccess = 4;
            const int maxRetriedBeforeFailure = 3;

            // Act / Assert
            var exception = Assert.Throws<RetryException>(() => _sut.PerformReturn(
                () => WaitForThirdPartyIngestion(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(10), maxRetriedBeforeFailure));

            exception?.Message.Should().Be("Problem with Ingestion Retrying");
        }

        #endregion

        #region Tests for Return Async Retry

        [Test]
        public async Task Retry_ReturnAsync_Successful()
        {
            // Arrange
            const int retriesTillSuccess = 2;
            const int maxRetriedBeforeFailure = 3;

            // Act
            var ingested = await _sut.PerformReturnAsync(
                async () => await WaitForThirdPartyIngestionAsync(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(10), maxRetriedBeforeFailure);

            _loggerMock.Verify(x => x.Warning(
                    "Retrying due to: {@Message} at {@DateTime}",
                    It.IsAny<string>(), It.IsAny<object>()),
                Times.Exactly(retriesTillSuccess));

            ingested.Should().BeTrue("Ingestion Async is successful");
        }

        [Test]
        public void Retry_ReturnAsync_Test_Failed()
        {
            // Arrange
            _retryAttempts = 0;
            const int retriesTillSuccess = 4;
            const int maxRetriedBeforeFailure = 2;

            // Act
            var exception = Assert.ThrowsAsync<RetryException>(async () => await _sut.PerformReturnAsync(
                async () => await WaitForThirdPartyIngestionAsync(_retryAttempts, retriesTillSuccess),
                TimeSpan.FromMilliseconds(10), maxRetriedBeforeFailure));

            exception?.Message.Should().Be("Problem with Ingestion Async Retrying");
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
                throw new RetryException("Located 'retry' in list Retrying");
            }
        }

        private static void ProcessThatDoesNotThrowRetry() => throw new ArgumentException("This Exception does not trigger a retry");

        private static async Task ProcessThatDoesNotThrowRetryAsync()
        {
            await Task.FromResult(true);
            throw new ArgumentException("This Exception does not trigger a retry");
        }

        private bool WaitForThirdPartyIngestion(int retryAttempt, int numberOfRetriesTillSuccess)
        {
            if (retryAttempt == numberOfRetriesTillSuccess)
                return true;

            _retryAttempts++;
            throw new RetryException("Problem with Ingestion Retrying");
        }

        private async Task<bool> WaitForThirdPartyIngestionAsync(int retryAttempt, int numberOfRetriesTillSuccess)
        {
            if (retryAttempt == numberOfRetriesTillSuccess)
                return await Task.FromResult(true);

            _retryAttempts++;
            throw new RetryException("Problem with Ingestion Async Retrying");
        }

        #endregion
    }
}