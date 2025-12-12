using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiDescriptionsRemoveNegativePhrases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A classic solid band ring with a smooth, even surface and medium width. The profile is gently rounded for everyday wear.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A simple Latin cross pendant with clean straight arms and a slightly elongated vertical bar.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A classic solid band ring with a smooth, even surface and medium width, without any stones or engravings. The profile is gently rounded for everyday wear.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A simple Latin cross pendant with clean straight arms and a slightly elongated vertical bar, without stones or engravings.");
        }
    }
}
