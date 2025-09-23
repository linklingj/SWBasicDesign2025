using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Scriptable Objects/ColorPalette")]
public class ColorPalette : ScriptableObject {
    [Serializable]
    public class Palette
    {
        [HideInInspector]
        public string Name;

        [LabelText("$Name")]
        [ListDrawerSettings(IsReadOnly = true, ShowFoldout = false)]
        public Color[] Colors;
    }

    public Palette blockTypeColorPalette;
    [ColorPalette]
    public Color forDebug;
    
#if UNITY_EDITOR
    [FoldoutGroup("Color Palettes"), Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void FetchColorPalettes()
    {
        blockTypeColorPalette = GetPalette("BlockType");
        if (blockTypeColorPalette == null)
            Debug.LogError("BlockType color palette not found.");
    }

    private Palette GetPalette(string paletteName)
    {
        var src = Sirenix.OdinInspector.Editor.ColorPaletteManager.Instance.ColorPalettes
            .Find(x => x.Name == paletteName);
        if (src == null) return null;
        return new Palette()
        {
            Name = paletteName,
            Colors = src.Colors.ToArray()
        };
    }
#endif
}