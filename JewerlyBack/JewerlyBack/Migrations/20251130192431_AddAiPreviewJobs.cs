using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPreviewJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPreviewJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SingleImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FramesJson = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPreviewJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiPreviewJobs_JewelryConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "JewelryConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiPreviewJobs_ConfigurationId",
                table: "AiPreviewJobs",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiPreviewJobs_Status",
                table: "AiPreviewJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiPreviewJobs");
        }
    }
}
