namespace Resilience.Retry
{
    public interface IResilienceRetry
    {
        /// <summary>
        /// Retries the specified action until a <c>RetryException</c> is not raised or the retries count is exceeded.
        /// <param name="action">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// <example>
        /// For example: Deleting all emails
        /// <code>
        /// homeDashboard.DeleteAllEmails();
        /// _resilienceRetry.Perform(() =>
        /// {
        ///     homeDashboard.Refresh();
        ///     var count = homeDashboard.RetrieveEmails().Count();
        ///     if (count is not 0)
        ///         throw new RetryException("Email count is not 0")
        /// }, o =>
        /// {
        ///    o.Delay = TimeSpan.FromSeconds(1);
        ///    o.Retries = 3;
        ///    o.LogRetries = true;
        /// });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        /// </summary>
        void Perform(Action action, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified asynchronous action until a <c>RetryException</c> is not raised or the retries count is exceeded.
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// <example>
        /// For example: Deleting all emails
        /// <code>
        /// homeDashboard.DeleteAllEmails();
        /// await _resilienceRetry.PerformAsync(async () =>
        /// {
        ///     await homeDashboard.RefreshAsync();
        ///     var count = (await homeDashboard.RetrieveEmailsAsync()).Count();
        ///     if (count is not 0)
        ///         throw new RetryException("Email count is not 0")
        /// }, o =>
        /// {
        ///    o.Delay = TimeSpan.FromSeconds(1);
        ///    o.Retries = 3;
        ///    o.LogRetries = true;
        /// });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        /// </summary>
        Task PerformAsync(Func<Task> func, Action<RetryOptions>? options = null);

        /// <summary>
        /// Retries the specified Func and returns
        /// <typeparam name="T">Returns - type from the Func</typeparam>
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for back end Email Creation request to complete
        /// <code>
        /// _emailSender.Create(10);
        /// var dashboardEmails = _resilienceRetry.PerformWithReturn(() =>
        /// {
        ///     homeDashboard.Refresh();
        ///     var emails = homeDashboard.RetrieveEmails();
        ///     if (emails.Count() is not 10)
        ///         throw new RetryException("Expecting email count of 10")
        ///
        ///     return emails;
        /// }, o =>
        /// {
        ///    o.Delay = TimeSpan.FromSeconds(1);
        ///    o.Retries = 3;
        ///    o.LogRetries = true;
        /// });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>

        T PerformWithReturn<T>(Func<T> func, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified asynchronous Func and returns
        /// <typeparam name="T">Returns - type from the Func</typeparam>
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for back end Email Creation request to complete
        /// <code>
        /// await _emailSender.CreateAsync(10);
        /// var dashboardEmails = await _resilienceRetry.PerformWithReturnAsync(async () =>
        /// {
        ///     await homeDashboard.RefreshAsync();
        ///     var emails = await homeDashboard.RetrieveEmailsAsync();
        ///     if (emails.Count() is not 10)
        ///         throw new RetryException("Expecting email count of 10")
        ///
        ///     return emails;
        /// }, o =>
        /// {
        ///    o.Delay = TimeSpan.FromSeconds(1);
        ///    o.Retries = 3;
        ///    o.LogRetries = true;
        /// });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        Task<T> PerformWithReturnAsync<T>(Func<Task<T>> func, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified Func until the outcome is True
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for a batch job to finish processing a report generation request
        /// <code>
        /// _reportManager.GenerateForId(81181);
        /// _resilienceRetry.UntilTrue("Waiting for report to be Generated",
        ///     () => reportManager.IsReportGenerated(81181),
        ///     o =>
        ///     {
        ///         o.Delay = TimeSpan.FromSeconds(1);
        ///         o.Retries = 3;
        ///         o.LogRetries = true;
        ///     });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        public void UntilTrue(string retryMessage, Func<bool> func, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified asynchronous Func until the outcome is True
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for a batch job to finish processing a report generation request
        /// <code>
        /// await _reportManager.GenerateForIdAsync(81181);
        /// await _resilienceRetry.UntilTrueAsync("Waiting for report to be Generated",
        ///     async () => await reportManager.IsReportGeneratedAsync(81181),
        ///     o =>
        ///     {
        ///         o.Delay = TimeSpan.FromSeconds(1);
        ///         o.Retries = 3;
        ///         o.LogRetries = true;
        ///     });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        public Task UntilTrueAsync(string retryMessage, Func<Task<bool>> func, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified Func until the outcome is False
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for a report to be deleted
        /// <code>
        /// _reportManager.DeleteId(81181);
        /// _resilienceRetry.UntilFalse("Waiting for report to be Deleted",
        ///     () => reportManager.IsDeleted(81181),
        ///     o =>
        ///     {
        ///         o.Delay = TimeSpan.FromSeconds(1);
        ///         o.Retries = 3;
        ///         o.LogRetries = true;
        ///     });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        public void UntilFalse(string retryMessage, Func<bool> func, Action<RetryOptions>? options = null);
        
        /// <summary>
        /// Retries the specified asynchronous Func until the outcome is False
        /// <param name="func">code to retry.</param>
        /// <param name="options">retry options e.g. delay, number of retries. Default: Delay - 1sec Retries - 5 (Optional)</param>
        /// </summary>
        /// <example>
        /// For example: Wait for a report to be deleted
        /// <code>
        /// await _reportManager.DeleteIdAsync(81181);
        /// await _resilienceRetry.UntilFalseAsync("Waiting for report to be Deleted",
        ///     () => reportManager.IsDeleted(81181),
        ///     o =>
        ///     {
        ///         o.Delay = TimeSpan.FromSeconds(1);
        ///         o.Retries = 3;
        ///         o.LogRetries = true;
        ///     });
        /// </code>
        /// A <c>RetryException</c> is thrown if a condition is not satisfied, this will trigger a retry if the <c>retries</c> count has not been exceeded
        /// </example>
        public Task UntilFalseAsync(string retryMessage, Func<Task<bool>> func, Action<RetryOptions>? options = null);
    }
}