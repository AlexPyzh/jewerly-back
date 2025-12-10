using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationStonesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_JewelryConfigurationStones_ConfigurationId",
                table: "JewelryConfigurationStones",
                newName: "IX_ConfigurationStones_ConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationStones_ConfigurationId",
                table: "JewelryConfigurationStones",
                newName: "IX_JewelryConfigurationStones_ConfigurationId");
        }
    }
}
