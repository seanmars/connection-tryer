namespace ConnectionTryerLibrary.Abstractions;

public interface IConnectionTryer
{
    TimeSpan Timeout { get; }
    bool EnableExceptionLogging { get; }

    IConnectionTryer WithTimeout(TimeSpan timeout);
    IConnectionTryer WithExceptionLogging(bool enable);
    Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}