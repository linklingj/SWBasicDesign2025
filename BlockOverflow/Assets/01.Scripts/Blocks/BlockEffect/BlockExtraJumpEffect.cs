using System;
using UnityEngine;

[Serializable]
public class BlockExtraJumpEffect : IBlockEffect
{
    public string EffectDescription => $"공중 점프 가능 횟수 +{extraJumpCount}";
    [SerializeField] private int extraJumpCount;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.extraJumpCount += extraJumpCount;
    }
}
