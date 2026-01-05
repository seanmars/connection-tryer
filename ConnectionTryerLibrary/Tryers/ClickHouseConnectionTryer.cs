using System.Diagnostics;
using ClickHouse.Client.ADO;
using ConnectionTryerLibrary.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConnectionTryerLibrary.Tryers;

public class ClickHouseConnectionTryer : IConnectionTryer
{
    private readonly string _connectionString;
    private readonly ILogger<ClickHouseConnectionTryer> _logger;

    public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(3);
    public bool EnableExceptionLogging { get; private set; } = true;

    public ClickHouseConnectionTryer(string connectionString, ILogger<ClickHouseConnectionTryer> logger)
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
        _logger.LogInformation("Starting ClickHouse connection test");
        var stopwatch = Stopwatch.StartNew();
        var result = new ConnectionTestResult();

        using var timeoutCts = new CancellationTokenSource(Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(linkedCts.Token);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(linkedCts.Token);

            result.IsSuccess = true;
            result.Message = "ClickHouse connection test succeeded";

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;
            _logger.LogInformation("ClickHouse connection test succeeded in {ElapsedTime}ms",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = $"ClickHouse connection test failed: {ex.Message}";
            result.Exception = ex;

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            if (EnableExceptionLogging)
            {
                _logger.LogError(ex, "ClickHouse connection test failed in {ElapsedTime}ms",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        return result;
    }
}