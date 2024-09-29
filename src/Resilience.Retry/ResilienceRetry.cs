using Polly;
using Polly.Retry;
using Serilog;

namespace Resilience.Retry
{
    public class ResilienceRetry : IResilienceRetry
    {
        private readonly ILogger _logger;

        public ResilienceRetry(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Perform(Action action, TimeSpan wait, int retries)
        {
            CreatePolicy(wait, retries)
                .Execute(action.Invoke);    
        }
    
        public async Task PerformAsync(Func<Task> func, TimeSpan wait, int retries)
        {
            await CreateAsyncPolicy(wait, retries)
                .ExecuteAsync(func.Invoke);
        }

        public T PerformWithReturn<T>(Func<T> func, TimeSpan wait, int retries)
        {
            var execution = CreatePolicy(wait, retries)
                .ExecuteAndCapture(func.Invoke);

            return execution.FinalException is null
                ? execution.Result
                : throw new RetryException(execution.FinalException.Message);
        }

        public async Task<T> PerformWithReturnAsync<T>(Func<Task<T>> func, TimeSpan wait, int retries)
        {
            return await CreateAsyncPolicy(wait, retries)
                .ExecuteAsync(func.Invoke);
        }

        public void UntilTrue(string retryMessage, Func<bool> func, TimeSpan wait, int retries)
            => PerformBooleanRetry(true, retryMessage, func, wait, retries);
        
        public async Task UntilTrueAsync(string retryMessage, Func<Task<bool>> func, TimeSpan wait, int retries)
            => await PerformBooleanRetryAsync(true, retryMessage, func, wait, retries);
        
        public void UntilFalse(string retryMessage, Func<bool> func, TimeSpan wait, int retries)
            => PerformBooleanRetry(false, retryMessage, func, wait, retries);
    
        public async Task UntilFalseAsync(string retryMessage, Func<Task<bool>> func, TimeSpan wait, int retries)
            => await PerformBooleanRetryAsync(false, retryMessage, func, wait, retries);
        
        private void PerformBooleanRetry(bool expectedOutcome, string retryMessage, Func<bool> func, TimeSpan timeSpan, int retries)
        {
            Perform(() =>
            {
                var outcome = func.Invoke();
                if (outcome == expectedOutcome)
                    return;

                throw new RetryException(retryMessage);
            }, timeSpan, retries);
        }
        
        private async Task PerformBooleanRetryAsync(bool expectedOutcome, string retryMessage, Func<Task<bool>> func, TimeSpan wait, int retries)
        {
            await PerformAsync(async () =>
            {
                var outcome = await func.Invoke();

                if (outcome != expectedOutcome)
                    throw new RetryException(retryMessage);
            }, wait, retries);
        }
        
        private AsyncRetryPolicy CreateAsyncPolicy(TimeSpan wait, int retries)
        {
            if (retries <=0)
                throw new ArgumentException("Retry count should be greater than zero");
            
            return Policy
                .Handle<RetryException>()
                .WaitAndRetryAsync(retries,
                    _ => wait,
                    (exception, _, _, _) => OnRetry(exception));
        }
    
        private Policy CreatePolicy(TimeSpan wait, int retries)
        {
            if (retries <=0)
                throw new ArgumentException("Retry count should be greater than zero");
            
            return Policy
                .Handle<RetryException>()
                .WaitAndRetry(retries,
                    _ => wait,
                    onRetry: (exception, _, _) =>OnRetry(exception));
        }

        private void OnRetry(Exception exception)
        {
            _logger.Warning(
                "Retrying due to: {@Message} at {@DateTime}", exception.Message, DateTime.Now.TimeOfDay);
        }
    }
}