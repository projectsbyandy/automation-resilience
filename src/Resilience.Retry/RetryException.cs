namespace Resilience.Retry
{
    public class RetryException(string message) : Exception(message);
}