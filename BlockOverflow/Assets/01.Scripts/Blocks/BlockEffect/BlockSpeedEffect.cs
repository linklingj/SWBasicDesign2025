using System;
using UnityEngine;

[Serializable]
public class BlockSpeedEffect : IBlockEffect
{
    public string EffectDescription => $"속도 +{speedAmount}";
    [SerializeField] private int speedAmount;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.speedIncrease += speedAmount;
    }
}
