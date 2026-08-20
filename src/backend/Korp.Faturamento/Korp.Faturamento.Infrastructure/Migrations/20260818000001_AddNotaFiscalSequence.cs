using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.Faturamento.Infrastructure.Migrations
{
    public partial class AddNotaFiscalSequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE SEQUENCE IF NOT EXISTS korp_faturamento.nota_fiscal_numero_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP SEQUENCE IF EXISTS korp_faturamento.nota_fiscal_numero_seq;");
        }
    }
}
