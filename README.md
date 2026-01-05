# Connection Tryer

一個用於測試資料庫連線的 .NET library,支援 MySQL、Redis 和 ClickHouse。

## 支援的資料庫

- MySQL
- Redis
- ClickHouse

## 安裝

將此 library 加入您的專案參考。

## 使用方式

### 1. 直接使用 Connection String

```csharp
using ConnectionTryerLibrary;

// MySQL
var mysqlTryer = new MySqlConnectionTryer("Server=localhost;Database=test;User=root;Password=password;");
var mysqlResult = await mysqlTryer.TestConnectionAsync();

// Redis
var redisTryer = new RedisConnectionTryer("localhost:6379");
var redisResult = await redisTryer.TestConnectionAsync();

// ClickHouse
var clickHouseTryer = new ClickHouseConnectionTryer("Host=localhost;Port=8123;Database=default;");
var clickHouseResult = await clickHouseTryer.TestConnectionAsync();

// 檢查結果
if (mysqlResult.IsSuccess)
{
    Console.WriteLine($"連線成功! 耗時: {mysqlResult.ElapsedTime.TotalMilliseconds}ms");
}
else
{
    Console.WriteLine($"連線失敗: {mysqlResult.Message}");
}
```

### 2. 使用 Dependency Injection (建議)

#### 註冊服務 (Program.cs 或 Startup.cs)

```csharp
using ConnectionTryerLibrary;

var builder = WebApplication.CreateBuilder(args);

// 註冊 ConnectionTryer 服務
builder.Services.AddConnectionTryer();

var app = builder.Build();
```

#### 使用注入的 Factory

```csharp
using ConnectionTryerLibrary;

public class ConnectionTestService
{
    private readonly IConnectionTryerFactory _factory;

    public ConnectionTestService(IConnectionTryerFactory factory)
    {
        _factory = factory;
    }

    public async Task TestConnectionsAsync()
    {
        // 從 appsettings.json 的 ConnectionStrings 區塊讀取
        var mysqlTryer = _factory.CreateMySql(); // 預設使用 "MySql" key
        var mysqlResult = await mysqlTryer.TestConnectionAsync();

        var redisTryer = _factory.CreateRedis(); // 預設使用 "Redis" key
        var redisResult = await redisTryer.TestConnectionAsync();

        var clickHouseTryer = _factory.CreateClickHouse(); // 預設使用 "ClickHouse" key
        var clickHouseResult = await clickHouseTryer.TestConnectionAsync();

        // 或者使用自訂 connection string name
        var customTryer = _factory.CreateMySql("MyCustomConnectionString");

        // 或者直接傳入 connection string
        var directTryer = _factory.Create(
            ConnectionType.MySql,
            "Server=localhost;Database=test;User=root;Password=password;"
        );
    }
}
```

## appsettings.json 設定範例

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=test;User=root;Password=password;",
    "Redis": "localhost:6379,password=yourpassword",
    "ClickHouse": "Host=localhost;Port=8123;Database=default;Username=default;Password=password;"
  }
}
```

## 完整範例

```csharp
using ConnectionTryerLibrary;

// 在 Program.cs 註冊服務
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConnectionTryer();
var app = builder.Build();

// 在 Controller 或 Service 中使用
public class HealthCheckController : ControllerBase
{
    private readonly IConnectionTryerFactory _factory;

    public HealthCheckController(IConnectionTryerFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("health/database")]
    public async Task<IActionResult> CheckDatabaseHealth()
    {
        var results = new Dictionary<string, ConnectionTestResult>();

        // 測試各個資料庫連線
        var mysqlTryer = _factory.CreateMySql();
        results["MySQL"] = await mysqlTryer.TestConnectionAsync();

        var redisTryer = _factory.CreateRedis();
        results["Redis"] = await redisTryer.TestConnectionAsync();

        var clickHouseTryer = _factory.CreateClickHouse();
        results["ClickHouse"] = await clickHouseTryer.TestConnectionAsync();

        // 檢查是否全部成功
        var allSuccess = results.Values.All(r => r.IsSuccess);

        return allSuccess ? Ok(results) : StatusCode(503, results);
    }
}
```

## ConnectionTestResult 屬性

- `IsSuccess` (bool): 連線是否成功
- `Message` (string): 測試結果訊息
- `Exception` (Exception?): 若失敗,包含例外資訊
- `ElapsedTime` (TimeSpan): 連線測試耗時

## NuGet 相依套件

- MySqlConnector (^2.4.0)
- StackExchange.Redis (^2.8.16)
- ClickHouse.Client (^7.11.0)
- Microsoft.Extensions.Configuration.Abstractions (^9.0.0)
- Microsoft.Extensions.DependencyInjection.Abstractions (^9.0.0)
