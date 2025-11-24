using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJewelryCategoriesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "rings", "Engagement and decorative rings", "Rings" });

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "earrings", "Various types of earrings", "Earrings" });

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "pendants", "Pendants and charms", "Pendants" });

            migrationBuilder.InsertData(
                table: "JewelryCategories",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 4, "necklaces", "Statement and delicate necklaces", true, "Necklaces" },
                    { 5, "bracelets", "Bangles and chain bracelets", true, "Bracelets" },
                    { 6, "chains", "Necklace and bracelet chains", true, "Chains" },
                    { 7, "brooches", "Decorative brooches and pins", true, "Brooches" },
                    { 8, "cufflinks", "Cufflinks and tie accessories", true, "Cufflinks" },
                    { 9, "piercing", "Body piercing jewelry", true, "Piercing" },
                    { 10, "hair_jewelry", "Hair accessories and ornaments", true, "Hair Jewelry" },
                    { 11, "sets", "Matching jewelry sets", true, "Sets" },
                    { 12, "mens_jewelry", "Jewelry designed for men", true, "Men's Jewelry" },
                    { 13, "custom", "Unique custom-made jewelry", true, "Custom Designs" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "ring", "Обручальные и декоративные кольца", "Кольца" });

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "pendant", "Подвески и кулоны", "Подвески" });

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "earring", "Серьги различных типов", "Серьги" });
        }
    }
}
