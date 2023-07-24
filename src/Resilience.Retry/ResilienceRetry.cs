using Polly;
using Polly.Retry;
using Serilog;

namespace Resilience.Retry;

public class ResilienceRetry : IResilienceRetry
{
    private readonly ILogger _logger;

    public ResilienceRetry(ILogger logger)
    {
        _logger = logger;
    }

    public void Perform(Action action, TimeSpan timeSpan, int retries)
    {
        CreatePolicy(timeSpan, retries)
            .Execute(action.Invoke);    
    }
    
    public async Task PerformAsync(Func<Task> func, TimeSpan wait, int retries)
    {
        await CreateAsyncPolicy(wait, retries)
            .ExecuteAsync(func.Invoke);
    }

    public T PerformReturn<T>(Func<T> func, TimeSpan timeSpan, int retries)
    {
        var execution = CreatePolicy(timeSpan, retries)
            .ExecuteAndCapture(func.Invoke);

        return execution.FinalException is null
            ? execution.Result
            : throw new RetryException(execution.FinalException.Message);
    }

    public async Task<T> PerformReturnAsync<T>(Func<Task<T>> func, TimeSpan timeSpan, int retries)
    {
        return await CreateAsyncPolicy(timeSpan, retries)
            .ExecuteAsync(func.Invoke);    }
    
    private AsyncRetryPolicy CreateAsyncPolicy(TimeSpan wait, int retries)
    {
        return Policy
            .Handle<RetryException>()
            .WaitAndRetryAsync(retries,
                _ => wait,
                (exception, _, _, _) => OnRetry(exception));
    }
    
    private Policy CreatePolicy(TimeSpan wait, int retries)
    {
        return Policy
            .Handle<RetryException>()
            .WaitAndRetry(retries,
                _ => wait,
                onRetry: (exception, _, _) =>
                {
                    OnRetry(exception);
                });
    }

    private void OnRetry(Exception exception)
    {
        _logger.Warning(
            "Retrying due to: {@Message} at {@DateTime}", exception.Message, DateTime.Now.TimeOfDay);
    }
}