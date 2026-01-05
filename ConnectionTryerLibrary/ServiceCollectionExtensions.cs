using ConnectionTryerLibrary.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionTryerLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionTryer(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionTryerFactory, ConnectionTryerFactory>();
        return services;
    }
}
