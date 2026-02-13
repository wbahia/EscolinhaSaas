using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EscolinhaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialPlanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "Planos",
                columns: new[] { "Id", "Ativo", "FeaturesJson", "MaxAlunos", "Nome", "Preco" },
                values: new object[,]
                {
                    { 1, true, "{}", 0, "Básico", 0m },
                    { 2, true, "{}", 0, "Profissional", 99.90m },
                    { 3, true, "{}", 0, "Premium", 199.90m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Planos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Planos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Planos",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
