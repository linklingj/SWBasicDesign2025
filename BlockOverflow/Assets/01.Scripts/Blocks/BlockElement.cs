using System;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class BlockElement : MonoBehaviour {
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetType(BlockType blockType)
    {
        //sr.color = blockType.ToColor();
    }
}

public class BlockElementEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        SirenixEditorGUI.Title("Block Element", null, TextAlignment.Left, true);
        SirenixEditorGUI.HorizontalLineSeparator(2);
        base.OnInspectorGUI();
    }
    
}