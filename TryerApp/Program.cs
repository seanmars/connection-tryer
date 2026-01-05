using ConnectionTryerLibrary;
using ConnectionTryerLibrary.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddConnectionTryer();

var host = builder.Build();
var factory = host.Services.GetRequiredService<IConnectionTryerFactory>();

var taskClickHouse = factory.CreateClickHouse().TestConnectionAsync();
var taskRedis = factory.CreateRedis().TestConnectionAsync();
var taskMySql = factory.CreateMySql().TestConnectionAsync();

await Task.WhenAll(taskClickHouse, taskRedis, taskMySql);