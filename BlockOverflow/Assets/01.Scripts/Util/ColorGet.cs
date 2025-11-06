using Sirenix.OdinInspector;
using UnityEngine;

public static class ColorGet {
    static ColorPalette palette;

    public static void SetPalette(ColorPalette p) => palette = p;

    public static Color ToColor(this BlockType blockType) {
        var c = TryGetPaletteColor(blockType);
        if (c.HasValue) return c.Value;
        return Color.gray;
    }
    
    public static Color ToColor(this Rarity rarity) {
        var c = TryGetPaletteColor(rarity);
        if (c.HasValue) return c.Value;
        return Color.gray;
    }

    private static bool EnsurePaletteLoaded() {
        if (palette != null) return true;
        palette = Resources.Load<ColorPalette>("ColorPalette");
        return palette != null;
    }

    private static Color? TryGetPaletteColor(BlockType blockType) {
        if (!EnsurePaletteLoaded()) return null;
        var pal = palette.blockTypeColorPalette;
        if (pal == null || pal.Colors == null) return null;
        int idx = (int)blockType;
        if ((uint)idx >= (uint)pal.Colors.Length) return null;
        return pal.Colors[idx];
    }
    
    private static Color? TryGetPaletteColor(Rarity rarity) {
        if (!EnsurePaletteLoaded()) return null;
        var pal = palette.blockTypeColorPalette;
        if (pal == null || pal.Colors == null) return null;
        int idx = (int)rarity;
        if ((uint)idx >= (uint)pal.Colors.Length) return null;
        return pal.Colors[idx];
    }
}