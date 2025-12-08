using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAiDescriptionToJewelryBaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiDescription",
                table: "JewelryBaseModels",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A classic solid band ring with a smooth, even surface and medium width, without any stones or engravings. The profile is gently rounded for everyday wear.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A delicate thin band ring with a single round stone set at the top in a simple prong or bezel setting, emphasizing minimalism and lightness.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "An engagement ring with a raised central setting holding a single larger stone, with a clean, elegant band that draws attention to the solitaire.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A narrow ring with a continuous line of evenly spaced small stones going all the way around the band, creating a uniform sparkle from every angle.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "AiDescription",
                value: "A solid, slightly heavier ring with a flat or gently curved top surface intended for a symbol or engraving, with a strong, masculine silhouette.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A small stud earring with a single stone or smooth disc sitting close to the earlobe, mounted on a straight post with a simple backing.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A stud earring formed by a tight cluster of several small stones or elements arranged into a compact shape that sits on the earlobe.");

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
                column: "AiDescription",
                value: "A medium-sized hoop earring with stones set along the visible outer front section of the hoop, combining a clean circular form with a line of sparkle.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "AiDescription",
                value: "A minimalist drop earring where a small pendant element hangs from a short connector, creating a light movement just below the earlobe.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A flat round disc pendant with a polished surface and a small bail at the top, ideal for engraving or subtle minimalist styling.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A narrow vertical bar pendant with clean straight edges and a slim rectangular shape, hanging from a small bail for a modern look.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A pendant in the shape of a heart outline with open center, made from a smooth metal contour that feels light and romantic.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A pendant built around a single central stone in a delicate setting, suspended from a small bail so the stone becomes the main focus.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"),
                column: "AiDescription",
                value: "A simple open circle pendant with a smooth round contour and empty center, symbolizing continuity and minimalism.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A fine chain necklace with a single small pendant fixed in the center so it always rests at the front of the neck.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A continuous line necklace made of closely set stones of similar size, forming a flexible sparkling band around the neck.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A short, close-fitting necklace that sits high on the neck, designed as a smooth, minimal band without large dangling elements.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A necklace with a horizontal plate or word element at the center of a fine chain, suitable for names or short inscriptions.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A classic chain bracelet composed of repeating metal links, flexible and lightweight, closing with a simple clasp.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A rigid bangle bracelet formed as a closed or nearly closed ring, with a smooth exterior surface and consistent thickness.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A bracelet made of a single row of evenly spaced stones set closely together, creating a continuous line of sparkle around the wrist.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A bracelet with a series of small pendants or charms attached to a base chain, allowing multiple decorative elements to dangle.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A simple cable chain made from uniform round or oval links connected in a straightforward pattern, suitable for pendants or standalone wear.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A curb chain with flattened, twisted links that lie smoothly against the skin, giving a slightly heavier and more masculine look.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A Figaro chain with a repeating pattern of one or two shorter links followed by a longer link, creating a rhythmic, stylish structure.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A rope chain built from twisted links that visually mimic a rope, with a textured, three-dimensional appearance and continuous spiral look.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"),
                column: "AiDescription",
                value: "A chain composed of small square or box-shaped links, forming a strong, geometric and slightly more structured profile.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A brooch shaped like a stylized flower with petals radiating from the center, designed to sit flat on fabric and add a decorative accent.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "An elongated bar-shaped brooch with clean geometric lines, often worn horizontally or diagonally as a subtle modern statement.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A brooch representing the silhouette or detailed figure of an animal, with contours and details emphasizing its character.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A brooch based on one or more letters intertwined into a decorative monogram, designed to stand out on clothing with refined lines.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A piercing jewelry piece with a flat disc on one end and a decorative top on the other, designed to sit flush against the skin.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A small, smooth hoop for piercing that forms a mostly closed circle, suitable for ears, nose, or other placements.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A straight bar piercing with a central shaft and a removable ball or decorative element on each end, used for tongue or ear piercings.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A gently curved barbell with beads or decorative ends, intended for areas like the eyebrow or navel where the curve follows the body.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A slim hair pin with a single stone or decorative element mounted at one end, meant to be partially visible in the hairstyle.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A hair pin with a small cluster of stones or elements arranged near the tip, creating a more pronounced decorative accent in the hair.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A short comb with multiple teeth that slide into the hair and a decorative top bar featuring stones or metal motifs.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A sleek bar-style hair clip with a clean rectangular front piece, designed to hold a section of hair with minimal visual clutter.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A substantial men's signet ring with a flat or lightly domed top face and thicker band, designed for a bold, classic masculine statement.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A medium-thickness chain necklace with sturdy links and a slightly heavier feel, intended for men's everyday wear.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A bracelet made of larger, stronger links that form a solid masculine chain around the wrist, closing with a robust clasp.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A pair of classic cufflinks with a flat or slightly domed decorative front face and a hinged back part that secures the cuff.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000001"),
                column: "AiDescription",
                value: "A simple Latin cross pendant with clean straight arms and a slightly elongated vertical bar, without stones or engravings.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000002"),
                column: "AiDescription",
                value: "A pendant in an Orthodox-style cross shape with characteristic additional crossbars and more intricate contours.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000003"),
                column: "AiDescription",
                value: "A cross pendant with small stones set along the arms, adding sparkle while preserving the clear cross silhouette.");

            migrationBuilder.UpdateData(
                table: "JewelryBaseModels",
                keyColumn: "Id",
                keyValue: new Guid("e0000000-0000-0000-0000-000000000004"),
                column: "AiDescription",
                value: "A very slim, minimal cross pendant with narrow arms and a lightweight appearance, emphasizing simplicity and elegance.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiDescription",
                table: "JewelryBaseModels");
        }
    }
}
