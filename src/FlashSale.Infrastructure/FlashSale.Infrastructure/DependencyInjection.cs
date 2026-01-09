using FlashSale.Core.Interfaces;
using FlashSale.Infrastructure.Data;
using FlashSale.Infrastructure.Redis;
using FlashSale.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FlashSale.Infrastructure;

/// <summary>
/// Extensões para configurar serviços de infraestrutura.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona serviços de infraestrutura ao container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="postgresConnectionString">String de conexão do PostgreSQL.</param>
    /// <param name="redisConnectionString">String de conexão do Redis.</param>
    /// <returns>Coleção de serviços configurada.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        string postgresConnectionString,
        string redisConnectionString = "localhost:6379")
    {
        // ════════════════════════════════════════════════════════════════
        // Entity Framework Core com PostgreSQL
        // ════════════════════════════════════════════════════════════════
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // ════════════════════════════════════════════════════════════════
        // Redis
        // ════════════════════════════════════════════════════════════════
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });

        // Redis Services
        services.AddSingleton<IStreamPublisher, RedisStreamPublisher>();
        services.AddSingleton<IStreamConsumer, RedisStreamConsumer>();
        services.AddSingleton<ICacheService, RedisCacheService>();

        // ════════════════════════════════════════════════════════════════
        // Repositories
        // ════════════════════════════════════════════════════════════════
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
