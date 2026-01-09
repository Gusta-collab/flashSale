using FlashSale.Infrastructure;
using FlashSale.Worker.Handlers;
using FlashSale.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// ════════════════════════════════════════════════════════════════════════
// SERVICES
// ════════════════════════════════════════════════════════════════════════

// Infrastructure (EF Core, Repositories, Redis)
var postgresConnectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? "Host=localhost;Port=5432;Database=flashsale;Username=postgres;Password=postgres";
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379";

builder.Services.AddInfrastructure(postgresConnectionString, redisConnectionString);

// Handler para processamento de pedidos
builder.Services.AddScoped<OrderProcessingHandler>();

// ════════════════════════════════════════════════════════════════════════
// HOSTED SERVICES
// ════════════════════════════════════════════════════════════════════════

// Consumer de pedidos do Redis Stream
builder.Services.AddHostedService<OrderConsumerService>();

var host = builder.Build();
host.Run();
