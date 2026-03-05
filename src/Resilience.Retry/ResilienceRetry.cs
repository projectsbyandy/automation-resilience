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
        
        public void Perform(Action action, Action<RetryOptions>? options = null)
        {
            CreatePolicy(options)
                .Execute(action.Invoke);    
        }
    
        public async Task PerformAsync(Func<Task> func, Action<RetryOptions>? options = null)
        {
            await CreateAsyncPolicy(options)
                .ExecuteAsync(func.Invoke);
        }

        public T PerformWithReturn<T>(Func<T> func, Action<RetryOptions>? options = null)
        {
            var execution = CreatePolicy(options)
                .ExecuteAndCapture(func.Invoke);

            return execution.FinalException is null
                ? execution.Result
                : throw new RetryException(execution.FinalException.Message);
        }

        public async Task<T> PerformWithReturnAsync<T>(Func<Task<T>> func, Action<RetryOptions>? options = null)
        {
            return await CreateAsyncPolicy(options)
                .ExecuteAsync(func.Invoke);
        }

        public void UntilTrue(string retryMessage, Func<bool> func, Action<RetryOptions>? options = null)
            => PerformBooleanRetry(true, retryMessage, func, options);
        
        public async Task UntilTrueAsync(string retryMessage, Func<Task<bool>> func, Action<RetryOptions>? options = null)
            => await PerformBooleanRetryAsync(true, retryMessage, func, options);
        
        public void UntilFalse(string retryMessage, Func<bool> func, Action<RetryOptions>? options = null)
            => PerformBooleanRetry(false, retryMessage, func, options);
    
        public async Task UntilFalseAsync(string retryMessage, Func<Task<bool>> func, Action<RetryOptions>? options = null)
            => await PerformBooleanRetryAsync(false, retryMessage, func, options);
        
        private void PerformBooleanRetry(bool expectedOutcome, string retryMessage, Func<bool> func, Action<RetryOptions>? options = null)
        {
            Perform(() =>
            {
                var outcome = func.Invoke();
                if (outcome == expectedOutcome)
                    return;

                throw new RetryException(retryMessage);
            }, options);
        }
        
        private async Task PerformBooleanRetryAsync(bool expectedOutcome, string retryMessage, Func<Task<bool>> func, Action<RetryOptions>? options = null)
        {
            await PerformAsync(async () =>
            {
                var outcome = await func.Invoke();

                if (outcome != expectedOutcome)
                    throw new RetryException(retryMessage);
            }, options);
        }
        
        private AsyncRetryPolicy CreateAsyncPolicy(Action<RetryOptions>? configure)
        {
            var options = ProcessOptions(configure);
            
            return Policy
                .Handle<RetryException>()
                .WaitAndRetryAsync(options.Retries,
                    _ => options.Delay,
                    (exception, _, _, _) => OnRetry(exception, options.LogRetries));
        }
    
        private Policy CreatePolicy(Action<RetryOptions>? configure)
        {
            var options = ProcessOptions(configure);
            
            return Policy
                .Handle<RetryException>()
                .WaitAndRetry(options.Retries,
                    _ => options.Delay,
                    onRetry: (exception, _, _) => OnRetry(exception, options.LogRetries));
        }

        private RetryOptions ProcessOptions(Action<RetryOptions>? configure)
        {
            var options = new RetryOptions();
            configure?.Invoke(options);
            
            options.Validate();

            return options;
        }

        private void OnRetry(Exception exception, bool logRetries)
        { 
            if (logRetries)
                _logger.Warning(
                    "Retrying due to: {@Message} at {@DateTime}", exception.Message, DateTime.Now.TimeOfDay);
        }
    }
}