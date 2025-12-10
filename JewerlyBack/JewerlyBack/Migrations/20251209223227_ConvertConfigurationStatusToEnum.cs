using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class ConvertConfigurationStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary integer column
            migrationBuilder.AddColumn<int>(
                name: "StatusInt",
                table: "JewelryConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Step 2: Map existing string values to integers
            // Draft = 0, ReadyToOrder = 1, InOrder = 2
            migrationBuilder.Sql(@"
                UPDATE ""JewelryConfigurations""
                SET ""StatusInt"" = CASE ""Status""
                    WHEN 'Draft' THEN 0
                    WHEN 'ReadyToOrder' THEN 1
                    WHEN 'InOrder' THEN 2
                    ELSE 0  -- Default to Draft for any unexpected values
                END
            ");

            // Step 3: Drop the old string column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "JewelryConfigurations");

            // Step 4: Rename the temporary column to Status
            migrationBuilder.RenameColumn(
                name: "StatusInt",
                table: "JewelryConfigurations",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary string column
            migrationBuilder.AddColumn<string>(
                name: "StatusStr",
                table: "JewelryConfigurations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft");

            // Step 2: Map integer values back to strings
            migrationBuilder.Sql(@"
                UPDATE ""JewelryConfigurations""
                SET ""StatusStr"" = CASE ""Status""
                    WHEN 0 THEN 'Draft'
                    WHEN 1 THEN 'ReadyToOrder'
                    WHEN 2 THEN 'InOrder'
                    ELSE 'Draft'  -- Default to Draft for any unexpected values
                END
            ");

            // Step 3: Drop the integer column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "JewelryConfigurations");

            // Step 4: Rename the temporary column to Status
            migrationBuilder.RenameColumn(
                name: "StatusStr",
                table: "JewelryConfigurations",
                newName: "Status");
        }
    }
}
