using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class CleanBaseModelDescriptionsRemoveStoneReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A delicate thin band ring with a small elevated setting structure at the top, emphasizing minimalism and lightness with a refined central focal point.", "thin_band_elevated_setting", "Delicate thin band featuring a minimal raised setting structure at the center, designed for a lightweight elegant appearance", "Thin Band with Elevated Setting" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A ring with a raised central setting structure featuring an elegant elevated profile, with a clean band that draws attention to the central focal point.", "classic_elevated_setting_ring", "Classic ring with a prominent raised central setting structure on a slender band, elegant silhouette with elevated focal point", "Classic Elevated Setting Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A narrow ring with a continuous line of evenly spaced small setting structures going all the way around the band, creating a uniform decorative pattern from every angle.", "continuous_setting_band", "Narrow band with a continuous row of evenly spaced small setting structures encircling the entire ring", "Continuous Setting Band" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A small stud earring with a decorative element or smooth disc sitting close to the earlobe, mounted on a straight post with a simple backing.", "Simple stud earring with a decorative element set directly on a post, minimalist and close to the earlobe" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A stud earring formed by a tight cluster of several small decorative elements arranged into a compact ornamental shape that sits on the earlobe.", "cluster_design_stud", "Stud earring featuring multiple small setting structures arranged in a tight decorative cluster pattern", "Cluster Design Stud" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A small, smooth hoop earring that hugs the earlobe closely, with a continuous circular or slightly oval shape and polished surface.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A medium-sized hoop earring with decorative setting structures along the visible outer front section of the hoop, combining a clean circular form with an embellished accent line.", "embellished_hoop", "Hoop earring with small decorative settings along the outer edge, adding visual interest to the classic hoop design", "Embellished Hoop" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "Description",
                value: "Earring with a decorative element suspended below the earlobe on a short chain or wire");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A flat round disc pendant with a polished surface and a small bail at the top, featuring clean minimalist styling.", "Flat circular disc pendant with smooth polished surface, versatile minimalist design" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A pendant built around a central elevated setting structure, suspended from a small bail so the setting becomes the main focus.", "elevated_setting_pendant", "Pendant with a central raised setting structure as the focal point, suspended from a chain", "Elevated Setting Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A continuous line necklace made of closely arranged uniform setting structures, forming a flexible decorative band around the neck.", "continuous_setting_necklace", "Continuous line of identical setting structures arranged side by side, creating an elegant collar effect", "Continuous Setting Necklace" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A necklace with a horizontal plate element at the center of a fine chain, suitable for personalization or decorative styling.", "Horizontal rectangular plate attached to a chain, designed for personalized text or decorative elements" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A bracelet made of a single row of evenly spaced setting structures linked closely together, creating a continuous decorative line around the wrist.", "continuous_setting_bracelet", "Line bracelet with a continuous row of individually linked setting structures", "Continuous Setting Bracelet" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "Description",
                value: "Decorative pin with flower-inspired design featuring petals radiating from a central focal point");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                column: "Description",
                value: "Decorative pin shaped like an animal or creature, with detailed metalwork and enamel accents");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A slim hair pin with a decorative element mounted at one end, meant to be partially visible in the hairstyle.", "decorative_top_hair_pin", "Simple hair pin with a decorative element at the top", "Decorative Top Hair Pin" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A hair pin with a small cluster of decorative elements arranged near the tip, creating a more pronounced ornamental accent in the hair.", "Hair pin featuring a cluster of decorative elements in an ornate design" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A short comb with multiple teeth that slide into the hair and a decorative top bar featuring ornamental metal motifs.", "Hair comb with decorative top edge embellished with metalwork patterns" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A cross pendant with small decorative settings along the arms, adding visual interest while preserving the clear cross silhouette.", "embellished_cross", "Latin cross with decorative setting structures along the beams or at intersection points", "Embellished Cross" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A delicate thin band ring with a single round stone set at the top in a simple prong or bezel setting, emphasizing minimalism and lightness.", "thin_band_single_stone", "Delicate thin band featuring one small stone set in a minimal prong or bezel setting", "Thin Band with Single Stone" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "An engagement ring with a raised central setting holding a single larger stone, with a clean, elegant band that draws attention to the solitaire.", "solitaire_engagement_ring", "Classic engagement ring with a prominent center stone elevated in a four or six-prong setting on a slender band", "Solitaire Engagement Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A narrow ring with a continuous line of evenly spaced small stones going all the way around the band, creating a uniform sparkle from every angle.", "eternity_ring", "Band fully encircled with small identical stones set side-by-side in a continuous channel or prong setting", "Eternity Ring" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A small stud earring with a single stone or smooth disc sitting close to the earlobe, mounted on a straight post with a simple backing.", "Simple stud earring with a single gemstone set directly on a post, minimalist and close to the earlobe" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A stud earring formed by a tight cluster of several small stones or elements arranged into a compact shape that sits on the earlobe.", "cluster_stud", "Stud earring featuring multiple small stones arranged in a tight cluster around a central stone", "Cluster Stud" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A small, smooth hoop earring that hugs the earlobe closely, with a continuous circular or slightly oval shape and no additional stones.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A medium-sized hoop earring with stones set along the visible outer front section of the hoop, combining a clean circular form with a line of sparkle.", "hoop_with_stones", "Hoop earring with small stones set along the outer edge, adding sparkle to the classic hoop design", "Hoop with Stones" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "Description",
                value: "Earring with a single stone or element suspended below the earlobe on a short chain or wire");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A flat round disc pendant with a polished surface and a small bail at the top, ideal for engraving or subtle minimalist styling.", "Flat circular disc pendant with smooth surface, ideal for engraving or minimal embellishment" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A pendant built around a single central stone in a delicate setting, suspended from a small bail so the stone becomes the main focus.", "single_stone_pendant", "Single prominent gemstone held in a prong or bezel setting, suspended from a chain as the focal point", "Single Stone Pendant" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A continuous line necklace made of closely set stones of similar size, forming a flexible sparkling band around the neck.", "tennis_necklace", "Continuous line of identical stones set in individual settings, creating a sparkling collar effect", "Tennis Necklace" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A necklace with a horizontal plate or word element at the center of a fine chain, suitable for names or short inscriptions.", "Horizontal rectangular plate attached to a chain, designed for personalized name or word engraving" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A bracelet made of a single row of evenly spaced stones set closely together, creating a continuous line of sparkle around the wrist.", "tennis_bracelet", "Line bracelet with a continuous row of individually set stones linked together", "Tennis Bracelet" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "Description",
                value: "Decorative pin with flower-inspired design featuring petals and possibly a center stone");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                column: "Description",
                value: "Decorative pin shaped like an animal or creature, often embellished with stones or enamel");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A slim hair pin with a single stone or decorative element mounted at one end, meant to be partially visible in the hairstyle.", "single_stone_hair_pin", "Simple hair pin with a single gemstone or decorative element at the top", "Single Stone Hair Pin" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A hair pin with a small cluster of stones or elements arranged near the tip, creating a more pronounced decorative accent in the hair.", "Hair pin featuring a cluster of small stones or floral elements in an ornate design" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Description" },
                values: new object[] { "A short comb with multiple teeth that slide into the hair and a decorative top bar featuring stones or metal motifs.", "Hair comb with decorative top edge embellished with stones or metal work" });

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000003"),
                columns: new[] { "AiDescription", "Code", "Description", "Name" },
                values: new object[] { "A cross pendant with small stones set along the arms, adding sparkle while preserving the clear cross silhouette.", "cross_with_stones", "Latin cross embellished with small gemstones set along the beams or at intersection points", "Cross with Stones" });
        }
    }
}
