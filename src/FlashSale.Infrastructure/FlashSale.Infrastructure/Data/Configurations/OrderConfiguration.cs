using FlashSale.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSale.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Order.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 2);

        builder.Property(o => o.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        builder.Property(o => o.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(o => o.UtmSource)
            .HasColumnName("utm_source")
            .HasMaxLength(100);

        builder.Property(o => o.UtmMedium)
            .HasColumnName("utm_medium")
            .HasMaxLength(100);

        builder.Property(o => o.UtmCampaign)
            .HasColumnName("utm_campaign")
            .HasMaxLength(100);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        // Índice único no IdempotencyKey (prevenir duplicados)
        builder.HasIndex(o => o.IdempotencyKey)
            .IsUnique();

        // Índices para queries frequentes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);

        // Relacionamento com Items
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
