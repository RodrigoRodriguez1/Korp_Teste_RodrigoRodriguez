using Korp.Faturamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Faturamento.Infrastructure.Persistence.Configurations;

internal sealed class ItemNotaConfiguration : IEntityTypeConfiguration<ItemNota>
{
    public void Configure(EntityTypeBuilder<ItemNota> builder)
    {
        builder.ToTable("itens_nota");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property<Guid>("nota_fiscal_id")
            .HasColumnName("nota_fiscal_id")
            .IsRequired();

        builder.Property(i => i.ProdutoId)
            .HasColumnName("produto_id")
            .IsRequired();

        builder.Property(i => i.ProdutoCodigo)
            .HasColumnName("produto_codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.ProdutoDescricao)
            .HasColumnName("produto_descricao")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantidade)
            .HasColumnName("quantidade")
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("chk_itens_nota_quantidade", "quantidade > 0"));
    }
}
