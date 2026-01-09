using FlashSale.Core.Interfaces;
using FlashSale.Infrastructure.Data;
using FlashSale.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="connectionString">String de conexão do PostgreSQL.</param>
    /// <returns>Coleção de serviços configurada.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        string connectionString)
    {
        // Entity Framework Core com PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
