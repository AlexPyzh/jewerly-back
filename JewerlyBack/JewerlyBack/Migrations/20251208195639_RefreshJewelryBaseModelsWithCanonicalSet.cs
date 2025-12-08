using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class RefreshJewelryBaseModelsWithCanonicalSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 250.0m, "classic_solid_band", "Simple solid metal band with smooth polished surface, uniform width throughout", null, "Classic Solid Band" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 400.0m, "thin_band_single_stone", "Delicate thin band featuring one small stone set in a minimal prong or bezel setting", null, "Thin Band with Single Stone" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 800.0m, "solitaire_engagement_ring", "Classic engagement ring with a prominent center stone elevated in a four or six-prong setting on a slender band", null, "Solitaire Engagement Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "classic_stud", "Simple stud earring with a single gemstone set directly on a post, minimalist and close to the earlobe", null, "Classic Stud" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 450.0m, "cluster_stud", "Stud earring featuring multiple small stones arranged in a tight cluster around a central stone", null, "Cluster Stud" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 220.0m, "small_hoop", "Smooth metal hoop in a small diameter, simple circular shape that hugs the earlobe", null, "Small Hoop" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 180.0m, "round_disc_pendant", "Flat circular disc pendant with smooth surface, ideal for engraving or minimal embellishment", null, "Round Disc Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 200.0m, "bar_pendant_vertical", "Slender vertical bar pendant with clean lines and modern aesthetic, suspended by the top edge", null, "Bar Pendant (Vertical)" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 220.0m, "heart_outline_pendant", "Open heart shape formed by a thin metal outline, romantic and delicate design", null, "Heart Outline Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 280.0m, "chain_small_central_pendant", "Delicate chain with a small decorative pendant or charm positioned at the center front", null, "Chain with Small Central Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 1200.0m, "tennis_necklace", "Continuous line of identical stones set in individual settings, creating a sparkling collar effect", null, "Tennis Necklace" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "chain_bracelet", "Flexible linked chain bracelet with clasp closure, elegant and adjustable", null, "Chain Bracelet" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "solid_bangle", "Rigid circular bracelet in solid metal, slips over the hand without a clasp", null, "Solid Bangle" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "Description", "MetadataJson" },
                values: new object[] { "tennis_bracelet", "Line bracelet with a continuous row of individually set stones linked together", null });

            migrationBuilder.InsertData(
                table: "JewelryBaseModels",
                columns: new[] { "Id", "BasePrice", "CategoryId", "Code", "Description", "IsActive", "MetadataJson", "Name", "PreviewImageUrl" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000004"), 950.0m, 1, "eternity_ring", "Band fully encircled with small identical stones set side-by-side in a continuous channel or prong setting", true, null, "Eternity Ring", null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), 450.0m, 1, "signet_ring", "Broad band with a flat rectangular or oval top surface suitable for engraving or decorative elements", true, null, "Signet Ring", null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), 550.0m, 2, "hoop_with_stones", "Hoop earring with small stones set along the outer edge, adding sparkle to the classic hoop design", true, null, "Hoop with Stones", null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), 380.0m, 2, "simple_drop_earring", "Earring with a single stone or element suspended below the earlobe on a short chain or wire", true, null, "Simple Drop Earring", null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), 350.0m, 3, "single_stone_pendant", "Single prominent gemstone held in a prong or bezel setting, suspended from a chain as the focal point", true, null, "Single Stone Pendant", null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), 190.0m, 3, "open_circle_pendant", "Circular ring pendant with open center, representing continuity and eternity", true, null, "Open Circle Pendant", null },
                    { new Guid("40000000-0000-0000-0000-000000000003"), 220.0m, 4, "minimal_choker_band", "Simple thin metal band that sits snugly around the neck, modern and streamlined", true, null, "Minimal Choker Band", null },
                    { new Guid("40000000-0000-0000-0000-000000000004"), 250.0m, 4, "name_plate_necklace", "Horizontal rectangular plate attached to a chain, designed for personalized name or word engraving", true, null, "Name Plate Necklace", null },
                    { new Guid("50000000-0000-0000-0000-000000000004"), 300.0m, 5, "charm_bracelet", "Chain bracelet with attachment points for hanging decorative charms or pendants", true, null, "Charm Bracelet", null },
                    { new Guid("60000000-0000-0000-0000-000000000001"), 150.0m, 6, "cable_chain", "Classic chain with uniform oval or round links connected in a simple alternating pattern", true, null, "Cable Chain", null },
                    { new Guid("60000000-0000-0000-0000-000000000002"), 170.0m, 6, "curb_chain", "Chain with interlocking uniform links that lie flat when worn, creating a smooth surface", true, null, "Curb Chain", null },
                    { new Guid("60000000-0000-0000-0000-000000000003"), 160.0m, 6, "figaro_chain", "Chain with alternating pattern of short and long oval links, typically three short links followed by one elongated link", true, null, "Figaro Chain", null },
                    { new Guid("60000000-0000-0000-0000-000000000004"), 200.0m, 6, "rope_chain", "Chain with small links twisted together to resemble rope texture, creating a thick and durable design", true, null, "Rope Chain", null },
                    { new Guid("60000000-0000-0000-0000-000000000005"), 180.0m, 6, "box_chain", "Chain with square links forming a smooth, continuous tube-like appearance", true, null, "Box Chain", null },
                    { new Guid("70000000-0000-0000-0000-000000000001"), 280.0m, 7, "floral_brooch", "Decorative pin with flower-inspired design featuring petals and possibly a center stone", true, null, "Floral Brooch", null },
                    { new Guid("70000000-0000-0000-0000-000000000002"), 220.0m, 7, "geometric_bar_brooch", "Horizontal bar pin with clean geometric lines and modern aesthetic", true, null, "Geometric Bar Brooch", null },
                    { new Guid("70000000-0000-0000-0000-000000000003"), 300.0m, 7, "animal_shape_brooch", "Decorative pin shaped like an animal or creature, often embellished with stones or enamel", true, null, "Animal Shape Brooch", null },
                    { new Guid("70000000-0000-0000-0000-000000000004"), 250.0m, 7, "monogram_brooch", "Pin featuring stylized initials or letters in an elegant font design", true, null, "Monogram Brooch", null },
                    { new Guid("90000000-0000-0000-0000-000000000001"), 120.0m, 9, "labret_stud", "Flat-back piercing stud with decorative front, suitable for lip, ear cartilage, or other piercings", true, null, "Labret Stud", null },
                    { new Guid("90000000-0000-0000-0000-000000000002"), 100.0m, 9, "hoop_piercing", "Circular or semi-circular hoop for various piercing locations, with secure closure mechanism", true, null, "Hoop Piercing", null },
                    { new Guid("90000000-0000-0000-0000-000000000003"), 90.0m, 9, "straight_barbell", "Straight bar with threaded balls or decorative ends on both sides, used for tongue, nipple, or industrial piercings", true, null, "Straight Barbell", null },
                    { new Guid("90000000-0000-0000-0000-000000000004"), 95.0m, 9, "curved_barbell", "Gently curved bar with threaded ends, commonly used for eyebrow, navel, or rook piercings", true, null, "Curved Barbell", null },
                    { new Guid("a0000000-0000-0000-0000-000000000001"), 80.0m, 10, "single_stone_hair_pin", "Simple hair pin with a single gemstone or decorative element at the top", true, null, "Single Stone Hair Pin", null },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), 120.0m, 10, "cluster_decorative_hair_pin", "Hair pin featuring a cluster of small stones or floral elements in an ornate design", true, null, "Cluster Decorative Hair Pin", null },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), 150.0m, 10, "decorative_hair_comb", "Hair comb with decorative top edge embellished with stones or metal work", true, null, "Decorative Hair Comb", null },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), 60.0m, 10, "minimal_bar_hair_clip", "Simple metal bar hair clip with clean lines and minimal decoration", true, null, "Minimal Bar Hair Clip", null },
                    { new Guid("c0000000-0000-0000-0000-000000000001"), 500.0m, 12, "mens_signet_ring", "Bold signet ring with wide band and large flat top surface, suitable for engraving or emblem", true, null, "Men's Signet Ring", null },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), 400.0m, 12, "mens_chain_necklace", "Substantial chain necklace with heavier gauge links, masculine and durable design", true, null, "Men's Chain Necklace", null },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), 450.0m, 12, "mens_link_bracelet", "Heavy link bracelet with robust construction and masculine proportions", true, null, "Men's Link Bracelet", null },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), 180.0m, 12, "classic_cufflinks", "Pair of dress shirt cufflinks with simple geometric shape, suitable for formal wear", true, null, "Classic Cufflinks", null },
                    { new Guid("e0000000-0000-0000-0000-000000000001"), 150.0m, 14, "plain_latin_cross", "Simple Latin cross with clean lines and smooth surface, traditional proportions with longer vertical beam", true, null, "Plain Latin Cross", null },
                    { new Guid("e0000000-0000-0000-0000-000000000002"), 180.0m, 14, "orthodox_style_cross", "Three-bar cross design in Orthodox tradition, featuring slanted lower bar and detailed proportions", true, null, "Orthodox-Style Cross", null },
                    { new Guid("e0000000-0000-0000-0000-000000000003"), 280.0m, 14, "cross_with_stones", "Latin cross embellished with small gemstones set along the beams or at intersection points", true, null, "Cross with Stones", null },
                    { new Guid("e0000000-0000-0000-0000-000000000004"), 120.0m, 14, "minimal_thin_cross", "Delicate cross with very thin wire-like construction, modern and understated design", true, null, "Minimal Thin Cross", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000004"));

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 500.0m, "ring_solitaire_classic", "Elegant thin band with a single center stone in prong setting", "{\"defaultRingSize\":16.5,\"bandWidth\":2.0}", "Classic Solitaire Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 800.0m, "ring_engagement_halo", "Center stone surrounded by a halo of smaller accent stones", "{\"defaultRingSize\":16.5,\"bandWidth\":2.5}", "Halo Engagement Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 400.0m, "ring_wide_band", "Modern wide band with smooth polished surface", "{\"defaultRingSize\":17.0,\"bandWidth\":5.0}", "Wide Band Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "earring_stud_classic", "Minimalist studs with a single gemstone", "{\"stoneSize\":5.0}", "Classic Stud Earrings" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 250.0m, "earring_hoop_medium", "Classic round hoops, medium size", "{\"diameter\":25.0}", "Medium Hoop Earrings" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 450.0m, "earring_drop_elegant", "Graceful drop earrings with dangling gemstone", "{\"length\":35.0}", "Elegant Drop Earrings" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 200.0m, "pendant_round_simple", "Simple round pendant with center stone", "{\"diameter\":15.0}", "Round Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 220.0m, "pendant_heart_classic", "Classic heart-shaped pendant", "{\"width\":12.0,\"height\":12.0}", "Heart Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 280.0m, "pendant_solitaire", "Single stone pendant in prong setting", "{\"stoneSize\":6.0}", "Solitaire Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 350.0m, "necklace_cable_chain", "Classic cable chain necklace", "{\"length\":45.0,\"linkSize\":3.0}", "Cable Chain Necklace" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                columns: new[] { "BasePrice", "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { 180.0m, "necklace_pendant_base", "Delicate chain designed for pendants", "{\"length\":42.0,\"linkSize\":2.0}", "Pendant Necklace Base" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "bracelet_chain_classic", "Simple elegant chain bracelet", "{\"length\":18.0,\"linkSize\":3.0}", "Classic Chain Bracelet" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                columns: new[] { "Code", "Description", "MetadataJson", "Name" },
                values: new object[] { "bracelet_bangle_simple", "Smooth round bangle bracelet", "{\"diameter\":65.0,\"width\":4.0}", "Simple Bangle Bracelet" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "Description", "MetadataJson" },
                values: new object[] { "bracelet_tennis", "Classic tennis bracelet with line of stones", "{\"length\":18.0,\"stoneCount\":20}" });
        }
    }
}
