namespace Resilience.Retry;

public interface IResilienceRetry
{
    void Perform(Action action, TimeSpan timeSpan, int retries);
    Task PerformAsync(Func<Task> func, TimeSpan wait, int retries);
    T PerformReturn<T>(Func<T> func, TimeSpan timeSpan, int retries);
    Task<T> PerformReturnAsync<T>(Func<Task<T>> func, TimeSpan timeSpan, int retries);
}