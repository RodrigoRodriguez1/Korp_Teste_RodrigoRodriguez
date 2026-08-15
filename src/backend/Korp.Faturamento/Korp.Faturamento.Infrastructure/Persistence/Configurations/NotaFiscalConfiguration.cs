using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Faturamento.Infrastructure.Persistence.Configurations;

internal sealed class NotaFiscalConfiguration : IEntityTypeConfiguration<NotaFiscal>
{
    public void Configure(EntityTypeBuilder<NotaFiscal> builder)
    {
        builder.ToTable("notas_fiscais");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property(n => n.Numero)
            .HasColumnName("numero")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(n => n.ImpressoEm)
            .HasColumnName("impresso_em");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(n => n.Itens)
            .WithOne()
            .HasForeignKey("nota_fiscal_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.Numero)
            .IsUnique()
            .HasDatabaseName("idx_notas_fiscais_numero");
    }
}
