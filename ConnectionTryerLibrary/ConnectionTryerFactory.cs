using ConnectionTryerLibrary.Abstractions;
using ConnectionTryerLibrary.Tryers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConnectionTryerLibrary;

public class ConnectionTryerFactory : IConnectionTryerFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public ConnectionTryerFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public IConnectionTryer Create(ConnectionType type, string connectionString)
    {
        return type switch
        {
            ConnectionType.MySql => new MySqlConnectionTryer(connectionString,
                _loggerFactory.CreateLogger<MySqlConnectionTryer>()),
            ConnectionType.Redis => new RedisConnectionTryer(connectionString,
                _loggerFactory.CreateLogger<RedisConnectionTryer>()),
            ConnectionType.ClickHouse => new ClickHouseConnectionTryer(connectionString,
                _loggerFactory.CreateLogger<ClickHouseConnectionTryer>()),
            _ => throw new ArgumentException($"Unsupported connection type: {type}", nameof(type))
        };
    }

    public IConnectionTryer CreateFromConfiguration(ConnectionType type, string connectionStringName)
    {
        var connectionString = _configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' not found in configuration");

        return Create(type, connectionString);
    }

    public IConnectionTryer CreateMySql(string connectionStringName = "MySql")
    {
        return CreateFromConfiguration(ConnectionType.MySql, connectionStringName);
    }

    public IConnectionTryer CreateRedis(string connectionStringName = "Redis")
    {
        return CreateFromConfiguration(ConnectionType.Redis, connectionStringName);
    }

    public IConnectionTryer CreateClickHouse(string connectionStringName = "ClickHouse")
    {
        return CreateFromConfiguration(ConnectionType.ClickHouse, connectionStringName);
    }
}