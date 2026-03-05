using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Resilience.Retry.Tests")]
namespace Resilience.Retry;

public class RetryOptions
{
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);
    public int Retries { get; set; } = 5;
    public bool LogRetries  { get; set; }
    
    internal void Validate()
    {
        if (Retries <= 0)
            throw new ArgumentException("Retries must be greater than zero.");

        if (Delay <= TimeSpan.Zero)
            throw new ArgumentException("Delay must be greater than zero.");
        
        if (Delay.Hours != 0)
            throw new ArgumentException("Delay must be less than 1 hour");        
    }
}