namespace ConnectionTryerLibrary.Abstractions;

public interface IConnectionTryerFactory
{
    IConnectionTryer Create(ConnectionType type, string connectionString);
    IConnectionTryer CreateFromConfiguration(ConnectionType type, string connectionStringName);
    IConnectionTryer CreateMySql(string connectionStringName = "MySql");
    IConnectionTryer CreateRedis(string connectionStringName = "Redis");
    IConnectionTryer CreateClickHouse(string connectionStringName = "ClickHouse");
}
