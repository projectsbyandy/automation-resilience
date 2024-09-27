namespace Resilience.Retry
{
    public interface IResilienceRetry
    {
        void Perform(Action action, TimeSpan wait, int retries);
        Task PerformAsync(Func<Task> func, TimeSpan wait, int retries);
        T PerformWithReturn<T>(Func<T> func, TimeSpan wait, int retries);
        Task<T> PerformWithReturnAsync<T>(Func<Task<T>> func, TimeSpan timeSpan, int retries);
        public void UntilTrue(string retryMessage, Func<bool> func, TimeSpan wait, int retries);
        public Task UntilTrueAsync(string retryMessage, Func<Task<bool>> func, TimeSpan wait, int retries);
        public void UntilFalse(string retryMessage, Func<bool> func, TimeSpan wait, int retries);
        public Task UntilTrueFalse(string retryMessage, Func<Task<bool>> func, TimeSpan wait, int retries);
    }
}