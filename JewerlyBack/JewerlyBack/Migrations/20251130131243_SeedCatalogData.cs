using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JewelryBaseModels",
                columns: new[] { "Id", "BasePrice", "CategoryId", "Code", "Description", "IsActive", "MetadataJson", "Name", "PreviewImageUrl" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), 500.0m, 1, "ring_solitaire_classic", "Elegant thin band with a single center stone in prong setting", true, "{\"defaultRingSize\":16.5,\"bandWidth\":2.0}", "Classic Solitaire Ring", null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), 800.0m, 1, "ring_engagement_halo", "Center stone surrounded by a halo of smaller accent stones", true, "{\"defaultRingSize\":16.5,\"bandWidth\":2.5}", "Halo Engagement Ring", null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), 400.0m, 1, "ring_wide_band", "Modern wide band with smooth polished surface", true, "{\"defaultRingSize\":17.0,\"bandWidth\":5.0}", "Wide Band Ring", null },
                    { new Guid("20000000-0000-0000-0000-000000000001"), 300.0m, 2, "earring_stud_classic", "Minimalist studs with a single gemstone", true, "{\"stoneSize\":5.0}", "Classic Stud Earrings", null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), 250.0m, 2, "earring_hoop_medium", "Classic round hoops, medium size", true, "{\"diameter\":25.0}", "Medium Hoop Earrings", null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), 450.0m, 2, "earring_drop_elegant", "Graceful drop earrings with dangling gemstone", true, "{\"length\":35.0}", "Elegant Drop Earrings", null },
                    { new Guid("30000000-0000-0000-0000-000000000001"), 200.0m, 3, "pendant_round_simple", "Simple round pendant with center stone", true, "{\"diameter\":15.0}", "Round Pendant", null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), 220.0m, 3, "pendant_heart_classic", "Classic heart-shaped pendant", true, "{\"width\":12.0,\"height\":12.0}", "Heart Pendant", null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), 280.0m, 3, "pendant_solitaire", "Single stone pendant in prong setting", true, "{\"stoneSize\":6.0}", "Solitaire Pendant", null },
                    { new Guid("40000000-0000-0000-0000-000000000001"), 350.0m, 4, "necklace_cable_chain", "Classic cable chain necklace", true, "{\"length\":45.0,\"linkSize\":3.0}", "Cable Chain Necklace", null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), 180.0m, 4, "necklace_pendant_base", "Delicate chain designed for pendants", true, "{\"length\":42.0,\"linkSize\":2.0}", "Pendant Necklace Base", null },
                    { new Guid("50000000-0000-0000-0000-000000000001"), 280.0m, 5, "bracelet_chain_classic", "Simple elegant chain bracelet", true, "{\"length\":18.0,\"linkSize\":3.0}", "Classic Chain Bracelet", null },
                    { new Guid("50000000-0000-0000-0000-000000000002"), 320.0m, 5, "bracelet_bangle_simple", "Smooth round bangle bracelet", true, "{\"diameter\":65.0,\"width\":4.0}", "Simple Bangle Bracelet", null },
                    { new Guid("50000000-0000-0000-0000-000000000003"), 950.0m, 5, "bracelet_tennis", "Classic tennis bracelet with line of stones", true, "{\"length\":18.0,\"stoneCount\":20}", "Tennis Bracelet", null }
                });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Name" },
                values: new object[] { "gold_14k_yellow", "14K Yellow Gold" });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "ColorHex", "Karat", "Name", "PriceFactor" },
                values: new object[] { "gold_18k_yellow", "#FFDF00", 18, "18K Yellow Gold", 1.2m });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "ColorHex", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[] { "gold_14k_white", "#E5E4E2", 14, "gold", "14K White Gold", 1.05m });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "ColorHex", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[] { "gold_18k_white", "#E8E8E8", 18, "gold", "18K White Gold", 1.25m });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "Code", "ColorHex", "IsActive", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[,]
                {
                    { 5, "gold_14k_rose", "#B76E79", true, 14, "gold", "14K Rose Gold", 1.05m },
                    { 6, "gold_18k_rose", "#C9A0A0", true, 18, "gold", "18K Rose Gold", 1.25m },
                    { 7, "platinum", "#E5E4E2", true, null, "platinum", "Platinum", 1.4m },
                    { 8, "silver_925", "#C0C0C0", true, null, "silver", "Sterling Silver 925", 0.6m },
                    { 9, "titanium", "#878681", true, null, "titanium", "Titanium", 0.8m }
                });

            migrationBuilder.UpdateData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Color", "DefaultPricePerCarat", "Name" },
                values: new object[] { "Clear", 5000.0m, "Diamond" });

            migrationBuilder.UpdateData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Color", "DefaultPricePerCarat", "Name" },
                values: new object[] { "Blue", 1500.0m, "Sapphire" });

            migrationBuilder.InsertData(
                table: "StoneTypes",
                columns: new[] { "Id", "Code", "Color", "DefaultPricePerCarat", "IsActive", "Name" },
                values: new object[,]
                {
                    { 3, "ruby", "Red", 1800.0m, true, "Ruby" },
                    { 4, "emerald", "Green", 2000.0m, true, "Emerald" },
                    { 5, "moissanite", "Clear", 400.0m, true, "Moissanite" },
                    { 6, "topaz", "Blue", 250.0m, true, "Topaz" },
                    { 7, "amethyst", "Purple", 150.0m, true, "Amethyst" },
                    { 8, "citrine", "Yellow", 180.0m, true, "Citrine" },
                    { 9, "aquamarine", "Light Blue", 300.0m, true, "Aquamarine" },
                    { 10, "garnet", "Deep Red", 200.0m, true, "Garnet" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Name" },
                values: new object[] { "gold_585_yellow", "Золото 585 жёлтое" });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "ColorHex", "Karat", "Name", "PriceFactor" },
                values: new object[] { "gold_585_white", "#E5E4E2", 14, "Золото 585 белое", 1.1m });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "ColorHex", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[] { "silver_925", "#C0C0C0", null, "silver", "Серебро 925", 0.3m });

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "ColorHex", "Karat", "MetalType", "Name", "PriceFactor" },
                values: new object[] { "platinum", "#E5E4E2", null, "platinum", "Платина", 2.5m });

            migrationBuilder.UpdateData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Color", "DefaultPricePerCarat", "Name" },
                values: new object[] { "Бесцветный", 50000.0m, "Бриллиант" });

            migrationBuilder.UpdateData(
                table: "StoneTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Color", "DefaultPricePerCarat", "Name" },
                values: new object[] { "Синий", 15000.0m, "Сапфир" });
        }
    }
}
