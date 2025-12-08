using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAiConfigJsonToAiPreviewJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiConfigJson",
                table: "AiPreviewJobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiConfigJson",
                table: "AiPreviewJobs");
        }
    }
}
