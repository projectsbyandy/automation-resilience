namespace Resilience.Retry;

public class RetryException : Exception
{
    public RetryException(string message) : base(message)
    {
    }
}