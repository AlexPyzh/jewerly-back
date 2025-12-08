using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoriesToCanonicalList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Hair jewelry");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Men's jewelry");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsActive",
                value: false);

            migrationBuilder.InsertData(
                table: "JewelryCategories",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[] { 14, "cross_pendants", "Religious cross pendants and charms", true, "Cross pendants" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Hair Jewelry");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Men's Jewelry");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsActive",
                value: true);
        }
    }
}
