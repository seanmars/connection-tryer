using System.Diagnostics;
using ConnectionTryerLibrary.Abstractions;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace ConnectionTryerLibrary.Tryers;

public class MySqlConnectionTryer : IConnectionTryer
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlConnectionTryer> _logger;
    
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(3);
    public bool EnableExceptionLogging { get; private set; } = true;

    public MySqlConnectionTryer(string connectionString, ILogger<MySqlConnectionTryer> logger)
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
        _logger.LogInformation("Starting MySQL connection test");
        var stopwatch = Stopwatch.StartNew();
        var result = new ConnectionTestResult();

        using var timeoutCts = new CancellationTokenSource(Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(linkedCts.Token);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(linkedCts.Token);

            result.IsSuccess = true;
            result.Message = "MySQL connection test succeeded";

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;
            _logger.LogInformation("MySQL connection test succeeded in {ElapsedTime}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = $"MySQL connection test failed: {ex.Message}";
            result.Exception = ex;

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            if (EnableExceptionLogging)
            {
                _logger.LogError(ex, "MySQL connection test failed in {ElapsedTime}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        return result;
    }
}
