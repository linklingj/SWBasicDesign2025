using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockAnimatorData", menuName = "Scriptable Objects/BlockAnimatorData")]
public class BlockAnimatorData : ScriptableObject
{
    [Title("등급별 보상 블록 테이블", "- 에서 사용", TitleAlignments.Centered)]

    [Header("Size")]
    public Vector3 shownSize;
    public Vector3 placedSize;
    public Vector3 dragSize;
    
    [Header("Position")]
    public Vector3 showPosition;
    public float generateSpread;
    public Vector3 selectedPosition;

    [Header("Animation Settings")]
    public float appearDuration = 0.35f;
    public float hoverDuration = 0.15f;
    public float selectDuration = 0.2f;
    public float dragDuration = 0.2f;
    public float dropDuration = 0.25f;
    public float disappearDuration = 0.2f;
    public float shakeStrength = 8f;
    public int shakeVibrato = 10;

    [Header("Camera Settings")] 
    public float inventoryTransitionDuration = 1f;
}
