using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestSupportToAiPreviewJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestClientId",
                table: "AiPreviewJobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AiPreviewJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPreviewJobs_GuestClientId_Status",
                table: "AiPreviewJobs",
                columns: new[] { "GuestClientId", "Status" },
                filter: "\"GuestClientId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiPreviewJobs_GuestClientId_Status",
                table: "AiPreviewJobs");

            migrationBuilder.DropColumn(
                name: "GuestClientId",
                table: "AiPreviewJobs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AiPreviewJobs");
        }
    }
}
