using System;
using UnityEngine;

[Serializable]
public class BlockJumpEffect : IBlockEffect
{
    public string EffectDescription => $"점프력 +{jumpAmount}";
    [SerializeField] private int jumpAmount;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.jumpIncrease += jumpAmount;
    }
}
