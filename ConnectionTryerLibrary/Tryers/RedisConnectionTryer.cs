using System.Diagnostics;
using ConnectionTryerLibrary.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConnectionTryerLibrary.Tryers;

public class RedisConnectionTryer : IConnectionTryer
{
    private readonly string _connectionString;
    private readonly ILogger<RedisConnectionTryer> _logger;
    
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(3);
    public bool EnableExceptionLogging { get; private set; } = true;

    public RedisConnectionTryer(string connectionString, ILogger<RedisConnectionTryer> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _connectionString = connectionString;
        _logger = logger;
    }

    public IConnectionTryer WithTimeout(TimeSpan timeout)
    {
        Timeout = timeout;
        return this;
    }

    public IConnectionTryer WithExceptionLogging(bool enable)
    {
        EnableExceptionLogging = enable;
        return this;
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Redis connection test");
        var stopwatch = Stopwatch.StartNew();
        var result = new ConnectionTestResult();
        ConnectionMultiplexer? connection = null;

        using var timeoutCts = new CancellationTokenSource(Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            var connectTask = ConnectionMultiplexer.ConnectAsync(_connectionString);
            await connectTask.WaitAsync(linkedCts.Token);
            connection = await connectTask;

            var database = connection.GetDatabase();
            await database.PingAsync();

            result.IsSuccess = true;
            result.Message = "Redis connection test succeeded";

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;
            _logger.LogInformation("Redis connection test succeeded in {ElapsedTime}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = $"Redis connection test failed: {ex.Message}";
            result.Exception = ex;

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            if (EnableExceptionLogging)
            {
                _logger.LogError(ex, "Redis connection test failed in {ElapsedTime}ms", stopwatch.ElapsedMilliseconds);
            }
        }
        finally
        {
            connection?.Dispose();
        }

        return result;
    }
}
