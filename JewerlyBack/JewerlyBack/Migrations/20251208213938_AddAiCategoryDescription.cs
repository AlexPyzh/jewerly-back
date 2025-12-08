using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewerlyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAiCategoryDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiCategoryDescription",
                table: "JewelryCategories",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "AiCategoryDescription",
                value: "A ring is a circular band worn on the finger, ranging from simple bands to elaborate designs with stones. Rings can be engagement rings with prominent center stones, wedding bands, fashion rings, or signet rings. They are designed to encircle the finger smoothly and can feature various widths, profiles, and decorative elements.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "AiCategoryDescription",
                value: "Earrings are jewelry pieces worn on or hanging from the earlobe or ear cartilage. They include studs that sit close to the ear, hoops that form circular shapes, drop earrings that dangle below the lobe, and more elaborate chandelier styles. Earrings are typically sold and worn as matching pairs.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "AiCategoryDescription",
                value: "A pendant is a decorative element designed to hang from a chain or cord around the neck. Pendants can be geometric shapes, symbols, stones in settings, or representational forms. They typically feature a bail or loop at the top for attachment and serve as the focal point of a necklace.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "AiCategoryDescription",
                value: "A necklace is a complete piece of jewelry that encircles the neck, including both the chain or structure and any decorative elements. Necklaces can be simple chains, tennis necklaces with continuous stones, chokers that sit high on the neck, or statement pieces with integrated pendants or decorative sections.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "AiCategoryDescription",
                value: "A bracelet is jewelry worn around the wrist, either as a flexible chain with a clasp or as a rigid bangle that slips over the hand. Bracelets can be delicate chains, tennis bracelets with continuous stones, charm bracelets with dangling elements, or solid cuffs and bangles.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "AiCategoryDescription",
                value: "A chain is a flexible series of connected metal links forming a continuous strand. Chains are worn as standalone jewelry or used as the foundation for pendants. Common styles include cable chains with round links, curb chains with flat links, rope chains with twisted construction, and box chains with square links.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "AiCategoryDescription",
                value: "A brooch is a decorative pin with a clasp mechanism on the back, designed to attach to clothing or fabric. Brooches can be floral designs, geometric shapes, animal figures, or abstract forms. They sit flat against fabric and serve as visible decorative accents on lapels, collars, or other garment areas.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "AiCategoryDescription",
                value: "Cufflinks are pairs of decorative fasteners used to secure the cuffs of dress shirts. They consist of a decorative front face connected to a backing mechanism that passes through buttonholes. Cufflinks are typically worn in formal settings and feature geometric, engraved, or stone-set designs.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "AiCategoryDescription",
                value: "Piercing jewelry is designed for body piercings beyond standard earlobe piercings, including cartilage, nose, lip, eyebrow, navel, and other locations. Common styles include labret studs with flat backs, small hoops, straight and curved barbells. These pieces are typically small, secure, and designed for continuous wear.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 10,
                column: "AiCategoryDescription",
                value: "Hair jewelry includes decorative accessories designed to adorn or secure hair. This includes hair pins with decorative tops, ornate hair combs with stones or metalwork, hair clips, and decorative barrettes. These pieces combine functionality with aesthetic appeal and are visible when worn in hairstyles.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 11,
                column: "AiCategoryDescription",
                value: "A jewelry set is a coordinated collection of matching pieces designed to be worn together, such as a necklace and earrings, or a ring and bracelet combination. Sets share design elements, materials, and style to create a cohesive look.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 12,
                column: "AiCategoryDescription",
                value: "Men's jewelry includes pieces designed with masculine proportions and aesthetic, such as heavier chain necklaces, substantial link bracelets, bold signet rings, and cufflinks. These pieces typically feature larger dimensions, stronger lines, and more substantial construction compared to traditional jewelry.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 13,
                column: "AiCategoryDescription",
                value: "Custom jewelry encompasses unique, made-to-order pieces designed specifically for an individual customer. These pieces can be any category but are characterized by personalized design elements, custom proportions, unique stone arrangements, or special engravings that make them one-of-a-kind creations.");

            migrationBuilder.UpdateData(
                table: "JewelryCategories",
                keyColumn: "Id",
                keyValue: 14,
                column: "AiCategoryDescription",
                value: "Cross pendants are religious or symbolic jewelry pieces in the shape of a cross, designed to hang from a chain. They range from simple Latin crosses with clean lines to elaborate Orthodox crosses with multiple bars, and can be plain metal or embellished with stones. Cross pendants serve both as expressions of faith and as decorative jewelry.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiCategoryDescription",
                table: "JewelryCategories");
        }
    }
}
