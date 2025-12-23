using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddUpgradeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpgradeAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OriginalImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JewelryType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DetectedCategoryId = table.Column<int>(type: "integer", nullable: true),
                    DetectedMetal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DetectedMetalDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasStones = table.Column<bool>(type: "boolean", nullable: false),
                    DetectedStonesJson = table.Column<string>(type: "text", nullable: true),
                    StyleClassification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfidenceScore = table.Column<double>(type: "double precision", precision: 5, scale: 4, nullable: false),
                    AnalysisDataJson = table.Column<string>(type: "text", nullable: true),
                    SuggestionsJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpgradeAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpgradeAnalyses_JewelryCategories_DetectedCategoryId",
                        column: x => x.DetectedCategoryId,
                        principalTable: "JewelryCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UpgradeAnalyses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UpgradePreviewJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    KeptOriginal = table.Column<bool>(type: "boolean", nullable: false),
                    AppliedSuggestionsJson = table.Column<string>(type: "text", nullable: true),
                    Prompt = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    EnhancedImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpgradePreviewJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpgradePreviewJobs_UpgradeAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "UpgradeAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UpgradePreviewJobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeAnalyses_DetectedCategoryId",
                table: "UpgradeAnalyses",
                column: "DetectedCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeAnalyses_GuestClientId",
                table: "UpgradeAnalyses",
                column: "GuestClientId",
                filter: "\"GuestClientId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeAnalyses_Status",
                table: "UpgradeAnalyses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeAnalyses_UserId",
                table: "UpgradeAnalyses",
                column: "UserId",
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradePreviewJobs_AnalysisId",
                table: "UpgradePreviewJobs",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradePreviewJobs_Status",
                table: "UpgradePreviewJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradePreviewJobs_UserId",
                table: "UpgradePreviewJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpgradePreviewJobs");

            migrationBuilder.DropTable(
                name: "UpgradeAnalyses");
        }
    }
}
