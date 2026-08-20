using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.Faturamento.Infrastructure.Migrations
{
    public partial class AddIdempotencyKeyExpiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                schema: "korp_faturamento",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() + INTERVAL '24 hours'");

            migrationBuilder.CreateIndex(
                name: "idx_idempotency_keys_expires_at",
                schema: "korp_faturamento",
                table: "idempotency_keys",
                column: "expires_at");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_idempotency_keys_expires_at",
                schema: "korp_faturamento",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "expires_at",
                schema: "korp_faturamento",
                table: "idempotency_keys");
        }
    }
}
