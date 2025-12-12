namespace JewerlyBack.Application.Ai;

/// <summary>
/// Resolves hex color codes to human-readable color names for AI prompts.
/// </summary>
public static class ColorNameResolver
{
    /// <summary>
    /// Known jewelry material colors mapped to descriptive names.
    /// </summary>
    private static readonly Dictionary<string, string> KnownMaterialColors = new(StringComparer.OrdinalIgnoreCase)
    {
        // Gold tones
        { "#FFD700", "bright yellow gold" },
        { "#D4AF37", "classic yellow gold" },
        { "#B8860B", "rich yellow gold" },
        { "#F5C563", "soft yellow gold" },

        // Rose gold tones
        { "#B76E79", "warm rose pink" },
        { "#E8B4B8", "soft rose gold" },
        { "#D4A5A5", "blush rose gold" },
        { "#C9A0A0", "dusty rose gold" },

        // White gold / platinum tones
        { "#E5E4E2", "silvery white" },
        { "#C0C0C0", "bright silver" },
        { "#D3D3D3", "light silver" },
        { "#A9A9A9", "cool gray silver" },
        { "#E8E8E8", "bright platinum" },

        // Sterling silver
        { "#C4CACE", "polished silver" },
        { "#AAA9AD", "sterling silver" },

        // Titanium
        { "#878681", "dark titanium gray" },
        { "#54534D", "deep titanium" }
    };

    /// <summary>
    /// Known gemstone colors mapped to descriptive names.
    /// </summary>
    private static readonly Dictionary<string, string> KnownStoneColors = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common stone color names to descriptive phrases
        { "clear", "brilliant clear" },
        { "white", "crystal clear" },
        { "colorless", "perfectly clear" },

        { "red", "vivid red" },
        { "deep red", "rich deep red" },
        { "dark red", "intense dark red" },

        { "blue", "bright blue" },
        { "deep blue", "rich sapphire blue" },
        { "light blue", "soft sky blue" },
        { "royal blue", "deep royal blue" },

        { "green", "vibrant green" },
        { "deep green", "lush deep green" },
        { "light green", "fresh light green" },

        { "purple", "deep purple" },
        { "violet", "rich violet" },
        { "lavender", "soft lavender" },

        { "pink", "delicate pink" },
        { "light pink", "soft blush pink" },
        { "hot pink", "vibrant pink" },

        { "yellow", "sunny yellow" },
        { "golden", "warm golden" },

        { "orange", "warm orange" },
        { "peach", "soft peach" },

        { "black", "deep black" },
        { "gray", "smoky gray" },
        { "grey", "smoky grey" }
    };

    /// <summary>
    /// Gets a human-readable color description for a material hex color.
    /// Falls back to analyzing the hex color if not in the known list.
    /// </summary>
    public static string GetMaterialColorDescription(string? hexColor, string metalType)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return GetDefaultColorForMetal(metalType);
        }

        // Try exact match first
        if (KnownMaterialColors.TryGetValue(hexColor, out var knownColor))
        {
            return knownColor;
        }

        // Fallback: analyze the hex color to determine a description
        return AnalyzeHexColor(hexColor, metalType);
    }

    /// <summary>
    /// Gets a human-readable color description for a stone color.
    /// </summary>
    public static string GetStoneColorDescription(string? color, string stoneType)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return GetDefaultColorForStone(stoneType);
        }

        // Try known color mapping
        if (KnownStoneColors.TryGetValue(color, out var description))
        {
            return description;
        }

        // If it's already descriptive (contains multiple words), use as is
        if (color.Contains(' '))
        {
            return color.ToLowerInvariant();
        }

        // Simple color - make it more descriptive
        return $"{color.ToLowerInvariant()}";
    }

    private static string GetDefaultColorForMetal(string metalType)
    {
        return metalType.ToLowerInvariant() switch
        {
            "gold" => "classic gold",
            "platinum" => "bright silvery white",
            "silver" => "polished silver",
            "titanium" => "dark gray",
            _ => "metallic"
        };
    }

    private static string GetDefaultColorForStone(string stoneType)
    {
        return stoneType.ToLowerInvariant() switch
        {
            "diamond" => "brilliant clear",
            "ruby" => "deep red",
            "sapphire" => "rich blue",
            "emerald" => "vibrant green",
            "amethyst" => "deep purple",
            "topaz" => "golden amber",
            "aquamarine" => "soft sea blue",
            "pearl" => "lustrous white",
            "opal" => "iridescent multicolor",
            "garnet" => "deep burgundy red",
            _ => "gemstone"
        };
    }

    private static string AnalyzeHexColor(string hexColor, string metalType)
    {
        try
        {
            var hex = hexColor.TrimStart('#');
            if (hex.Length != 6) return GetDefaultColorForMetal(metalType);

            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);

            // Determine the dominant characteristic
            var avg = (r + g + b) / 3;
            var isLight = avg > 180;
            var isMedium = avg > 100 && avg <= 180;

            // Check for rose/pink tones (red > green, low blue difference)
            if (r > g && r > b && Math.Abs(r - b) < 80)
            {
                return isLight ? "soft rose gold" : "warm rose pink";
            }

            // Check for yellow gold tones (red and green high, blue low)
            if (r > 180 && g > 150 && b < 150)
            {
                return isLight ? "bright yellow gold" : "rich yellow gold";
            }

            // Check for silver/white tones (all values close together and high)
            if (Math.Abs(r - g) < 30 && Math.Abs(g - b) < 30 && Math.Abs(r - b) < 30)
            {
                if (isLight) return "bright silvery white";
                if (isMedium) return "cool silver gray";
                return "dark metallic";
            }

            // Default based on metal type
            return GetDefaultColorForMetal(metalType);
        }
        catch
        {
            return GetDefaultColorForMetal(metalType);
        }
    }
}
