using Korp.Faturamento.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Faturamento.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(k => k.Key);

        builder.Property(k => k.Key)
            .HasColumnName("key")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(k => k.ResponseBody)
            .HasColumnName("response_body")
            .IsRequired();

        builder.Property(k => k.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
