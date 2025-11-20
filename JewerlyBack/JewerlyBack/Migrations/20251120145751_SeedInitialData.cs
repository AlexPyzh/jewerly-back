using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JewelryCategories",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "ring", "Обручальные и декоративные кольца", true, "Кольца" },
                    { 2, "pendant", "Подвески и кулоны", true, "Подвески" },
                    { 3, "earring", "Серьги различных типов", true, "Серьги" }
                });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "Code", "ColorHex", "IsActive", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[,]
                {
                    { 1, "gold_585_yellow", "#FFD700", true, 14, "gold", "Золото 585 жёлтое", 1.0m },
                    { 2, "gold_585_white", "#E5E4E2", true, 14, "gold", "Золото 585 белое", 1.1m },
                    { 3, "silver_925", "#C0C0C0", true, null, "silver", "Серебро 925", 0.3m },
                    { 4, "platinum", "#E5E4E2", true, null, "platinum", "Платина", 2.5m }
                });

            migrationBuilder.InsertData(
                table: "StoneTypes",
                columns: new[] { "Id", "Code", "Color", "DefaultPricePerCarat", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "diamond", "Бесцветный", 50000.0m, true, "Бриллиант" },
                    { 2, "sapphire", "Синий", 15000.0m, true, "Сапфир" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
