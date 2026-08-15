using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.Faturamento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "faturamento");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "faturamento",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    response_body = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                schema: "faturamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    impresso_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itens_nota",
                schema: "faturamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotaFiscalId = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    produto_descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    nota_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_nota", x => x.id);
                    table.CheckConstraint("chk_itens_nota_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "FK_itens_nota_notas_fiscais_nota_fiscal_id",
                        column: x => x.nota_fiscal_id,
                        principalSchema: "faturamento",
                        principalTable: "notas_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_nota_nota_fiscal_id",
                schema: "faturamento",
                table: "itens_nota",
                column: "nota_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "idx_notas_fiscais_numero",
                schema: "faturamento",
                table: "notas_fiscais",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_keys",
                schema: "faturamento");

            migrationBuilder.DropTable(
                name: "itens_nota",
                schema: "faturamento");

            migrationBuilder.DropTable(
                name: "notas_fiscais",
                schema: "faturamento");
        }
    }
}
